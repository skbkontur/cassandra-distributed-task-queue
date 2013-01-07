using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

using GroBuf;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Connections;

using log4net;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public class GetEventsEnumerator : IEnumerator<string>
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
                ColumnInfo columnInfo = TicksNameHelper.GetColumnInfo(taskState, TicksNameHelper.GetMinimalTicksForRow(iCur), "");
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

        public string Current
        {
            get
            {
                var taskId = serializer.Deserialize<string>(eventEnumerator.Current.Value);
                return taskId;
            }
        }

        object IEnumerator.Current { get { return Current; } }

        private void LogFromToCountStatistics()
        {
            totalDifferenceFromTo += (iTo - iFrom);
            totalCount++;
            if(lastLogDateTime <= DateTime.UtcNow - TimeSpan.FromMinutes(1))
            {
                logger.InfoFormat("Statistics about a number of requested rows. Mean number of processed rows = {0}", (double)totalDifferenceFromTo / (totalCount + 1));
                lastLogDateTime = DateTime.UtcNow;
                totalDifferenceFromTo = 0;
                totalCount = 0;
            }
        }

        private void UpdateTicks()
        {
            long ticks = TicksNameHelper.GetTicksFromColumnName(eventEnumerator.Current.Name) - 1;
            minTicksCache.UpdateMinTicks(taskState, ticks);
        }

        private static long totalDifferenceFromTo;
        private static long totalCount;
        private static readonly ILog logger = LogManager.GetLogger(typeof(GetEventsEnumerator));
        private static DateTime lastLogDateTime = DateTime.UtcNow - TimeSpan.FromMinutes(1);

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
    }
}