using System;
using System.Collections;
using System.Collections.Generic;

using GroBuf;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Cassandra.CassandraClient.Connections;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public class GetEventsEnumerable : IEnumerable<Tuple<string, ColumnInfo>>
    {
        public GetEventsEnumerable(TaskState taskState, ISerializer serializer, IColumnFamilyConnection connection, IMinTicksCache minTicksCache, long fromTicks, long toTicks, int batchSize)
        {
            this.taskState = taskState;
            this.serializer = serializer;
            this.connection = connection;
            this.minTicksCache = minTicksCache;
            this.fromTicks = fromTicks;
            this.toTicks = toTicks;
            this.batchSize = batchSize;
        }

        public IEnumerator<Tuple<string, ColumnInfo>> GetEnumerator()
        {
            return new GetEventsEnumerator(taskState, serializer, connection, minTicksCache, fromTicks, toTicks, batchSize);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private readonly TaskState taskState;
        private readonly ISerializer serializer;
        private readonly IColumnFamilyConnection connection;
        private readonly IMinTicksCache minTicksCache;
        private readonly long fromTicks;
        private readonly long toTicks;
        private readonly int batchSize;
    }
}