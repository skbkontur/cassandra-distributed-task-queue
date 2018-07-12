using System.Collections;
using System.Collections.Generic;

using GroBuf;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Connections;
using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.Cassandra.Repositories
{
    public class GetEventLogEnumerator : IEnumerator<TaskMetaUpdatedEvent>
    {
        public GetEventLogEnumerator(ISerializer serializer, IColumnFamilyConnection connection, long fromTicks, long toTicks, int batchSize)
        {
            this.serializer = serializer;
            this.connection = connection;
            this.fromTicks = fromTicks;
            this.batchSize = batchSize;
            iFrom = fromTicks / EventPointerFormatter.PartitionDurationTicks;
            iTo = (toTicks + EventPointerFormatter.PartitionDurationTicks / 3) / EventPointerFormatter.PartitionDurationTicks;
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
                if (eventEnumerator.MoveNext())
                    return true;
                if (iCur >= iTo)
                    return false;
                iCur++;
                string startColumnName = null;
                if (iCur == iFrom)
                    startColumnName = EventPointerFormatter.GetColumnName(fromTicks, GuidHelpers.MinGuid);
                var partitionKey = EventPointerFormatter.GetPartitionKey(iCur * EventPointerFormatter.PartitionDurationTicks);
                eventEnumerator = connection.GetRow(partitionKey, startColumnName, batchSize).GetEnumerator();
            }
        }

        public void Reset()
        {
            iCur = iFrom - 1;
            eventEnumerator = new List<Column>().GetEnumerator();
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
    }
}