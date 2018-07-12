using System.Collections;
using System.Collections.Generic;

using GroBuf;

using RemoteQueue.Cassandra.Entities;

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
}