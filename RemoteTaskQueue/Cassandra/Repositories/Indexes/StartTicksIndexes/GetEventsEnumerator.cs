using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using GroBuf;

using JetBrains.Annotations;

using log4net;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Connections;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public class GetEventsEnumerator : IEnumerator<TaskIndexRecord>
    {
        public GetEventsEnumerator([NotNull] TaskNameAndState taskNameAndState, ISerializer serializer, IColumnFamilyConnection connection, IOldestLiveRecordTicksHolder oldestLiveRecordTicksHolder, long fromTicks, long toTicks, int batchSize)
        {
            this.taskNameAndState = taskNameAndState;
            this.serializer = serializer;
            this.connection = connection;
            this.oldestLiveRecordTicksHolder = oldestLiveRecordTicksHolder;
            this.fromTicks = fromTicks;
            this.toTicks = toTicks;
            this.batchSize = batchSize;
            iFrom = TicksNameHelper.GetTicksRowNumber(fromTicks);
            iTo = TicksNameHelper.GetTicksRowNumber(toTicks);
            Reset();
            LogFromToCountStatistics();
        }

        public void Dispose()
        {
            eventEnumerator.Dispose();
        }

        public bool MoveNext()
        {
            while(true)
            {
                if(eventEnumerator.MoveNext())
                {
                    var currentLiveRecordTicks = TicksNameHelper.GetTicksFromColumnName(eventEnumerator.Current.Name);
                    if(currentLiveRecordTicks > toTicks)
                    {
                        oldestLiveRecordTicksHolder.TryMoveForward(taskNameAndState, toTicks);
                        return false;
                    }
                    oldestLiveRecordTicksHolder.TryMoveForward(taskNameAndState, currentLiveRecordTicks);
                    if(!loggedTooOldIndexRecord && currentLiveRecordTicks < (DateTime.UtcNow - TimeSpan.FromHours(1)).Ticks)
                    {
                        logger.WarnFormat("Too old index record: [TaskId = {0}, ColumnName = {1}, ColumnTimestamp = {2}]",
                                          Current.TaskId, eventEnumerator.Current.Name, eventEnumerator.Current.Timestamp);
                        loggedTooOldIndexRecord = true;
                    }
                    return true;
                }
                if(iCur >= iTo)
                {
                    oldestLiveRecordTicksHolder.TryMoveForward(taskNameAndState, toTicks);
                    return false;
                }
                iCur++;
                var rowKey = TicksNameHelper.GetRowKey(taskNameAndState, TicksNameHelper.GetMinimalTicksForRow(iCur));
                string exclusiveStartColumnName = null;
                if(iCur == iFrom)
                    exclusiveStartColumnName = TicksNameHelper.GetColumnName(fromTicks, string.Empty);
                eventEnumerator = connection.GetRow(rowKey, exclusiveStartColumnName, batchSize).GetEnumerator();
            }
        }

        public void Reset()
        {
            iCur = iFrom - 1;
            eventEnumerator = (new List<Column>()).GetEnumerator();
        }

        [NotNull]
        public TaskIndexRecord Current
        {
            get
            {
                var taskId = serializer.Deserialize<string>(eventEnumerator.Current.Value);
                var minimalStartTicks = TicksNameHelper.GetTicksFromColumnName(eventEnumerator.Current.Name);
                return new TaskIndexRecord(taskId, minimalStartTicks, taskNameAndState);
            }
        }

        object IEnumerator.Current { get { return Current; } }

        private void LogFromToCountStatistics()
        {
            lock(statisticsLockObject)
            {
                if(statistics == null)
                    statistics = new Dictionary<TaskNameAndState, TaskStateStatistics>();
                if(!statistics.ContainsKey(taskNameAndState))
                    statistics[taskNameAndState] = new TaskStateStatistics();
                statistics[taskNameAndState].Update(iTo - iFrom);
                if(lastStatisticsLogDateTime <= DateTime.UtcNow - TimeSpan.FromMinutes(1))
                {
                    PrintStatistics();
                    statistics = new Dictionary<TaskNameAndState, TaskStateStatistics>();
                    lastStatisticsLogDateTime = DateTime.UtcNow;
                }
            }
        }

        private static void PrintStatistics()
        {
            var result = new StringBuilder();
            result.AppendLine("Statistics about a number of requested rows:");
            foreach(var statistic in statistics)
                result.AppendLine(string.Format(" {0} {1}", statistic.Key, (double)statistic.Value.TotalProcessedRows / (statistic.Value.TotalCount + 1)));
            logger.InfoFormat(result.ToString());
        }

        private static Dictionary<TaskNameAndState, TaskStateStatistics> statistics;
        private static readonly object statisticsLockObject = new object();

        private static readonly ILog logger = LogManager.GetLogger(typeof(GetEventsEnumerator));
        private static DateTime lastStatisticsLogDateTime = DateTime.UtcNow - TimeSpan.FromMinutes(1);

        private readonly TaskNameAndState taskNameAndState;
        private readonly ISerializer serializer;
        private readonly IColumnFamilyConnection connection;
        private readonly IOldestLiveRecordTicksHolder oldestLiveRecordTicksHolder;
        private readonly long fromTicks;
        private readonly long toTicks;
        private readonly int batchSize;
        private readonly long iFrom;
        private readonly long iTo;
        private long iCur;
        private bool loggedTooOldIndexRecord;
        private IEnumerator<Column> eventEnumerator;

        private class TaskStateStatistics
        {
            public void Update(long processedRows)
            {
                TotalProcessedRows += processedRows;
                TotalCount++;
            }

            public long TotalProcessedRows { get; private set; }
            public long TotalCount { get; private set; }
        }
    }
}