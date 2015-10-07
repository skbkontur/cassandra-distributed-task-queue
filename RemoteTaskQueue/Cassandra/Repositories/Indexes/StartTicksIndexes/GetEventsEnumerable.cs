using System;
using System.Collections;
using System.Collections.Generic;

using GroBuf;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Cassandra.CassandraClient.Connections;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public class GetEventsEnumerable : IEnumerable<Tuple<string, TaskColumnInfo>>
    {
        public GetEventsEnumerable(TaskState taskState, ISerializer serializer, IColumnFamilyConnection connection, IFromTicksProvider fromTicksProvider, long fromTicks, long toTicks, int batchSize)
        {
            this.taskState = taskState;
            this.serializer = serializer;
            this.connection = connection;
            this.fromTicksProvider = fromTicksProvider;
            this.fromTicks = fromTicks;
            this.toTicks = toTicks;
            this.batchSize = batchSize;
        }

        public IEnumerator<Tuple<string, TaskColumnInfo>> GetEnumerator()
        {
            return new GetEventsEnumerator(taskState, serializer, connection, fromTicksProvider, fromTicks, toTicks, batchSize);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private readonly TaskState taskState;
        private readonly ISerializer serializer;
        private readonly IColumnFamilyConnection connection;
        private readonly IFromTicksProvider fromTicksProvider;
        private readonly long fromTicks;
        private readonly long toTicks;
        private readonly int batchSize;
    }
}