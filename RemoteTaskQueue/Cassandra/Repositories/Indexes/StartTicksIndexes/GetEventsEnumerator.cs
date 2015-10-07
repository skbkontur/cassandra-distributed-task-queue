using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using GroBuf;

using log4net;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Connections;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public class GetEventsEnumerator : IEnumerator<Tuple<string, TaskColumnInfo>>
    {
        public GetEventsEnumerator(TaskState taskState, ISerializer serializer, IColumnFamilyConnection connection, IFromTicksProvider fromTicksProvider, long fromTicks, long toTicks, int batchSize)
        {
            this.taskState = taskState;
            this.serializer = serializer;
            this.connection = connection;
            this.fromTicksProvider = fromTicksProvider;
            this.fromTicks = fromTicks;
            this.batchSize = batchSize;
            toTicksString = (toTicks + 1).ToString("D20", CultureInfo.InvariantCulture);
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
                    if(eventEnumerator.Current.Name.CompareTo(toTicksString) >= 0)
                    {
                        if(startPosition)
                            fromTicksProvider.UpdateMinTicks(taskState, DateTime.UtcNow.Ticks);
                        return false;
                    }
                    if(startPosition)
                    {
                        UpdateTicks();
                        startPosition = false;
                    }
                    return true;
                }
                if(iCur >= iTo)
                {
                    if(startPosition)
                        fromTicksProvider.UpdateMinTicks(taskState, DateTime.UtcNow.Ticks);
                    return false;
                }
                iCur++;
                string exclusiveStartColumnName = null;
                var columnInfo = TicksNameHelper.GetColumnInfo(taskState, TicksNameHelper.GetMinimalTicksForRow(iCur), string.Empty);
                if(iCur == iFrom)
                    exclusiveStartColumnName = TicksNameHelper.GetColumnInfo(taskState, fromTicks, string.Empty).ColumnName;
                eventEnumerator = connection.GetRow(columnInfo.RowKey, exclusiveStartColumnName, batchSize).GetEnumerator();
            }
        }

        public void Reset()
        {
            startPosition = true;
            iCur = iFrom - 1;
            eventEnumerator = (new List<Column>()).GetEnumerator();
        }

        public Tuple<string, TaskColumnInfo> Current
        {
            get
            {
                var taskId = serializer.Deserialize<string>(eventEnumerator.Current.Value);
                var columnName = eventEnumerator.Current.Name;
                var rowKey = TicksNameHelper.GetRowKey(taskState, TicksNameHelper.GetTicksFromColumnName(columnName));
                var columnInfo = new TaskColumnInfo(rowKey, columnName);
                return new Tuple<string, TaskColumnInfo>(taskId, columnInfo);
            }
        }

        object IEnumerator.Current { get { return Current; } }

        private void LogFromToCountStatistics()
        {
            lock(statisticsLockObject)
            {
                if(statistics == null)
                    statistics = new Dictionary<TaskState, TaskStateStatistics>();
                if(!statistics.ContainsKey(taskState))
                    statistics[taskState] = new TaskStateStatistics();
                statistics[taskState].Update(iTo - iFrom);
                if(lastStatisticsLogDateTime <= DateTime.UtcNow - TimeSpan.FromMinutes(1))
                {
                    PrintStatistics();
                    statistics = new Dictionary<TaskState, TaskStateStatistics>();
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

        private void UpdateTicks()
        {
            var ticks = TicksNameHelper.GetTicksFromColumnName(eventEnumerator.Current.Name) - 1;
            fromTicksProvider.UpdateMinTicks(taskState, ticks);

            if(ticks < (DateTime.UtcNow - TimeSpan.FromHours(1)).Ticks)
            {
                var current = Current;
                logger.WarnFormat("Too old index record: [TaskId = {0}, ColumnName = {1}, ColumnTimestamp = {2}]",
                                  current.Item1, eventEnumerator.Current.Name, eventEnumerator.Current.Timestamp);
            }
        }

        private static Dictionary<TaskState, TaskStateStatistics> statistics;
        private static readonly object statisticsLockObject = new object();

        private static readonly ILog logger = LogManager.GetLogger(typeof(GetEventsEnumerator));
        private static DateTime lastStatisticsLogDateTime = DateTime.UtcNow - TimeSpan.FromMinutes(1);

        private readonly TaskState taskState;
        private readonly ISerializer serializer;
        private readonly IColumnFamilyConnection connection;
        private readonly IFromTicksProvider fromTicksProvider;
        private readonly long fromTicks;
        private readonly string toTicksString;
        private readonly int batchSize;
        private readonly long iFrom;
        private readonly long iTo;
        private long iCur;
        private bool startPosition;
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