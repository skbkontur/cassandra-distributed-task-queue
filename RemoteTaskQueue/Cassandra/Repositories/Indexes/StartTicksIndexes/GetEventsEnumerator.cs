using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using GroBuf;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Connections;

using log4net;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public class GetEventsEnumerator : IEnumerator<Tuple<string, ColumnInfo>>
    {
        public GetEventsEnumerator(TaskState taskState, ISerializer serializer, IColumnFamilyConnection connection, IMinTicksCache minTicksCache, long fromTicks, long toTicks, int batchSize)
        {
            this.taskState = taskState;
            this.serializer = serializer;
            this.connection = connection;
            this.minTicksCache = minTicksCache;
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
                            minTicksCache.UpdateMinTicks(taskState, DateTime.UtcNow.Ticks);
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
                        minTicksCache.UpdateMinTicks(taskState, DateTime.UtcNow.Ticks);
                    return false;
                }
                iCur++;
                string startColumnName = null;
                var columnInfo = TicksNameHelper.GetColumnInfo(taskState, TicksNameHelper.GetMinimalTicksForRow(iCur), "");
                if(iCur == iFrom) startColumnName = TicksNameHelper.GetColumnInfo(taskState, fromTicks, "").ColumnName;
                eventEnumerator = connection.GetRow(columnInfo.RowKey, startColumnName, batchSize).GetEnumerator();
            }
        }

        public void Reset()
        {
            startPosition = true;
            iCur = iFrom - 1;
            eventEnumerator = (new List<Column>()).GetEnumerator();
        }

        public Tuple<string, ColumnInfo> Current
        {
            get
            {
                var taskId = serializer.Deserialize<string>(eventEnumerator.Current.Value);
                var columnName = eventEnumerator.Current.Name;
                var columnInfo = new ColumnInfo
                    {
                        ColumnName = columnName,
                        RowKey = TicksNameHelper.GetRowName(taskState, TicksNameHelper.GetTicksFromColumnName(columnName))
                    };
                return new Tuple<string, ColumnInfo>(taskId, columnInfo);
            }
        }

        object IEnumerator.Current { get { return Current; } }

        private void LogFromToCountStatistics()
        {
            lock (statisticsLockObject)
            {
                if (statistics == null)
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
            minTicksCache.UpdateMinTicks(taskState, ticks);
        }

        private static Dictionary<TaskState, TaskStateStatistics> statistics;
        private static readonly object statisticsLockObject = new object();

        private static readonly ILog logger = LogManager.GetLogger(typeof(GetEventsEnumerator));
        private static DateTime lastStatisticsLogDateTime = DateTime.UtcNow - TimeSpan.FromMinutes(1);

        private readonly TaskState taskState;
        private readonly ISerializer serializer;
        private readonly IColumnFamilyConnection connection;
        private readonly IMinTicksCache minTicksCache;
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