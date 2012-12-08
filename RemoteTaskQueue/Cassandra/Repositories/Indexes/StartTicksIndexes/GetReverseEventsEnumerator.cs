using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using GroBuf;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Connections;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public class GetReverseEventsEnumerator : IEnumerator<string>
    {
        public GetReverseEventsEnumerator(TaskState taskState, ISerializer serializer, IColumnFamilyConnection connection, IMinTicksCache minTicksCache, long fromTicks, long toTicks, int batchSize)
        {
            this.taskState = taskState;
            this.serializer = serializer;
            this.connection = connection;
            this.minTicksCache = minTicksCache;
            this.fromTicks = fromTicks;
            this.batchSize = batchSize;
            fromTicksString = fromTicks.ToString("D20", CultureInfo.InvariantCulture);
            toTicksString = (toTicks + 1).ToString("D20", CultureInfo.InvariantCulture);
            iFrom = TicksNameHelper.GetTicksRowNumber(fromTicks);
            iTo = TicksNameHelper.GetTicksRowNumber(toTicks);
            Reset();
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
                    if(eventEnumerator.Current.Name.CompareTo(fromTicksString) < 0) // не включаем левую границу
                    {
                        if(startPosition)
                            minTicksCache.UpdateMinTicks(taskState, DateTime.UtcNow.Ticks);
                        return false;
                    }
                    UpdateTicks();
                    startPosition = false;
                    return true;
                }

                if(iCur <= iFrom)
                {
                    if(startPosition)
                        minTicksCache.UpdateMinTicks(taskState, DateTime.UtcNow.Ticks);
                    return false;
                }
                iCur--;
                ColumnInfo columnInfo = TicksNameHelper.GetColumnInfo(taskState, TicksNameHelper.GetMinimalTicksForRow(iCur), "");
                var x = connection.GetRow(columnInfo.RowKey, batchSize);
                var y = (iCur == iTo ? x.Where(column => column.Name.CompareTo(toTicksString) <0) : x).Reverse();
                eventEnumerator = y.GetEnumerator();
            }
        }

        public void Reset()
        {
            startPosition = true;
            iCur = iTo + 1;
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

        private void UpdateTicks()
        {
            long ticks = TicksNameHelper.GetTicksFromColumnName(eventEnumerator.Current.Name) - 1; // не понятно..
            minTicksCache.UpdateMinTicks(taskState, ticks);
        }

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
        private readonly string fromTicksString;
    }
}