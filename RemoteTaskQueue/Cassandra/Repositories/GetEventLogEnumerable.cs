using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

using GroBuf;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Connections;

namespace RemoteQueue.Cassandra.Repositories
{
    public class GetEventLogEnumerable : IEnumerable<TaskMetaUpdatedEvent>
    {
        public GetEventLogEnumerable(ISerializer serializer, IColumnFamilyConnection connection, long fromTicks, long toTicks, int batchSize)
        {
            this.serializer = serializer;
            this.connection = connection;
            this.fromTicks = fromTicks;
            this.toTicks = toTicks;
            this.batchSize = batchSize;
        }

        public IEnumerator<TaskMetaUpdatedEvent> GetEnumerator()
        {
            return new GetEventLogEnumerator(serializer, connection, fromTicks, toTicks, batchSize);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private readonly ISerializer serializer;
        private readonly IColumnFamilyConnection connection;
        private readonly long fromTicks;
        private readonly long toTicks;
        private readonly int batchSize;
    }




    public class GetEventLogEnumerator : IEnumerator<TaskMetaUpdatedEvent>
    {
        public GetEventLogEnumerator(ISerializer serializer, IColumnFamilyConnection connection, long fromTicks, long toTicks, int batchSize)
        {
            this.serializer = serializer;
            this.connection = connection;
            this.fromTicks = fromTicks;
            this.batchSize = batchSize;
            iFrom = fromTicks / tickPartition;
            iTo = (toTicks + tickPartition / 3) / tickPartition;
            Reset();
        }

        public void Dispose()
        {
            eventEnumerator.Dispose();
        }

        public bool MoveNext()
        {
            while (true)
            {
                if (eventEnumerator.MoveNext()) return true;
                if (iCur >= iTo) return false;
                iCur++;
                string startColumnName = null;
                var columnInfo = GetColumnInfo(iCur * tickPartition);
                if (iCur == iFrom) startColumnName = GetColumnInfo(fromTicks).Item2;
                eventEnumerator = connection.GetRow(columnInfo.Item1, startColumnName, batchSize).GetEnumerator();
            }
        }


        private static Tuple<string, string> GetColumnInfo(long ticks)
        {
            var rowKey = (ticks / tickPartition).ToString();
            var columnName = ticks.ToString("D20", CultureInfo.InvariantCulture);
            return new Tuple<string, string>(rowKey, columnName);
        }

        public void Reset()
        {
            iCur = iFrom - 1;
            eventEnumerator = (new List<Column>()).GetEnumerator();
        }

        public TaskMetaUpdatedEvent Current
        {
            get
            {
                var xmlContent = eventEnumerator.Current.Value;
                return serializer.Deserialize<TaskMetaUpdatedEvent>(xmlContent);
            }
        }

        object IEnumerator.Current { get { return Current; } }

        private readonly ISerializer serializer;
        private readonly IColumnFamilyConnection connection;
        private readonly long fromTicks;
        private readonly int batchSize;
        private readonly long iFrom;
        private readonly long iTo;
        private long iCur;
        private IEnumerator<Column> eventEnumerator;
        private static readonly long tickPartition = TimeSpan.FromMinutes(6).Ticks;
    }
}