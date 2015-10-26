using System.Collections;
using System.Collections.Generic;

using GroBuf;

using JetBrains.Annotations;

using SKBKontur.Cassandra.CassandraClient.Connections;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public class GetEventsEnumerable : IEnumerable<TaskIndexRecord>
    {
        public GetEventsEnumerable([NotNull] TaskTopicAndState taskTopicAndState, ISerializer serializer, IColumnFamilyConnection connection, IOldestLiveRecordTicksHolder oldestLiveRecordTicksHolder, long fromTicks, long toTicks, int batchSize)
        {
            this.taskTopicAndState = taskTopicAndState;
            this.serializer = serializer;
            this.connection = connection;
            this.oldestLiveRecordTicksHolder = oldestLiveRecordTicksHolder;
            this.fromTicks = fromTicks;
            this.toTicks = toTicks;
            this.batchSize = batchSize;
        }

        public IEnumerator<TaskIndexRecord> GetEnumerator()
        {
            return new GetEventsEnumerator(taskTopicAndState, serializer, connection, oldestLiveRecordTicksHolder, fromTicks, toTicks, batchSize);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private readonly TaskTopicAndState taskTopicAndState;
        private readonly ISerializer serializer;
        private readonly IColumnFamilyConnection connection;
        private readonly IOldestLiveRecordTicksHolder oldestLiveRecordTicksHolder;
        private readonly long fromTicks;
        private readonly long toTicks;
        private readonly int batchSize;
    }
}