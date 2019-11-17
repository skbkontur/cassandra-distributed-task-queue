using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using GroBuf;

using JetBrains.Annotations;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Connections;

using SkbKontur.Cassandra.TimeBasedUuid;

using Vostok.Logging.Abstractions;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public class GetEventsEnumerator : IEnumerator<TaskIndexRecord>
    {
        public GetEventsEnumerator([NotNull] ILiveRecordTicksMarker liveRecordTicksMarker,
                                   ISerializer serializer,
                                   IColumnFamilyConnection connection,
                                   long fromTicks,
                                   long toTicks,
                                   int batchSize,
                                   ILog logger)
        {
            this.liveRecordTicksMarker = liveRecordTicksMarker;
            this.serializer = serializer;
            this.connection = connection;
            this.fromTicks = fromTicks;
            this.toTicks = toTicks;
            this.batchSize = batchSize;
            this.logger = logger;
            iFrom = CassandraNameHelper.GetTicksRowNumber(fromTicks);
            iTo = CassandraNameHelper.GetTicksRowNumber(toTicks);
            Reset();
            LogFromToCountStatistics();
        }

        public void Dispose()
        {
            eventEnumerator.Dispose();
            liveRecordTicksMarker.CommitChanges();
        }

        public bool MoveNext()
        {
            while (true)
            {
                if (eventEnumerator.MoveNext())
                {
                    var currentLiveRecordTicks = CassandraNameHelper.GetTicksFromColumnName(eventEnumerator.Current.Name);
                    if (currentLiveRecordTicks > toTicks)
                    {
                        liveRecordTicksMarker.TryMoveForward(toTicks);
                        return false;
                    }
                    liveRecordTicksMarker.TryMoveForward(currentLiveRecordTicks);
                    if (!loggedTooOldIndexRecord && currentLiveRecordTicks < (Timestamp.Now - TimeSpan.FromHours(1)).Ticks)
                    {
                        logger.Warn(string.Format("Too old index record: [TaskId = {0}, ColumnName = {1}, ColumnTimestamp = {2}]",
                                                  Current.TaskId, eventEnumerator.Current.Name, eventEnumerator.Current.Timestamp));
                        loggedTooOldIndexRecord = true;
                    }
                    return true;
                }
                if (iCur >= iTo)
                {
                    liveRecordTicksMarker.TryMoveForward(toTicks);
                    return false;
                }
                iCur++;
                var rowKey = CassandraNameHelper.GetRowKey(liveRecordTicksMarker.State.TaskIndexShardKey, CassandraNameHelper.GetMinimalTicksForRow(iCur));
                string exclusiveStartColumnName = null;
                if (iCur == iFrom)
                    exclusiveStartColumnName = CassandraNameHelper.GetColumnName(fromTicks, string.Empty);
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
                var minimalStartTicks = CassandraNameHelper.GetTicksFromColumnName(eventEnumerator.Current.Name);
                return new TaskIndexRecord(taskId, minimalStartTicks, liveRecordTicksMarker.State.TaskIndexShardKey);
            }
        }

        object IEnumerator.Current => Current;

        private void LogFromToCountStatistics()
        {
            lock (statisticsLockObject)
            {
                if (statistics == null)
                    statistics = new Dictionary<TaskIndexShardKey, TaskStateStatistics>();
                var taskIndexShardKey = liveRecordTicksMarker.State.TaskIndexShardKey;
                if (!statistics.ContainsKey(taskIndexShardKey))
                    statistics[taskIndexShardKey] = new TaskStateStatistics();
                statistics[taskIndexShardKey].Update(iTo - iFrom);
                if (lastStatisticsLogMoment <= Timestamp.Now - TimeSpan.FromMinutes(1))
                {
                    PrintStatistics();
                    statistics = new Dictionary<TaskIndexShardKey, TaskStateStatistics>();
                    lastStatisticsLogMoment = Timestamp.Now;
                }
            }
        }

        private void PrintStatistics()
        {
            var result = new StringBuilder();
            result.AppendLine("Statistics about a number of requested rows:");
            foreach (var statistic in statistics)
                result.AppendLine(string.Format(" {0} {1}", statistic.Key, (double)statistic.Value.TotalProcessedRows / (statistic.Value.TotalCount + 1)));
            logger.Info(result.ToString());
        }

        private static Dictionary<TaskIndexShardKey, TaskStateStatistics> statistics;
        private static readonly object statisticsLockObject = new object();

        private static Timestamp lastStatisticsLogMoment = Timestamp.Now - TimeSpan.FromMinutes(1);

        private readonly ILiveRecordTicksMarker liveRecordTicksMarker;
        private readonly ISerializer serializer;
        private readonly IColumnFamilyConnection connection;
        private readonly long fromTicks;
        private readonly long toTicks;
        private readonly int batchSize;
        private readonly long iFrom;
        private readonly long iTo;
        private readonly ILog logger;
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