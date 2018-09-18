using System.Collections;
using System.Collections.Generic;

using GroBuf;

using JetBrains.Annotations;

using SKBKontur.Cassandra.CassandraClient.Connections;

using Vostok.Logging.Abstractions;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public class GetEventsEnumerable : IEnumerable<TaskIndexRecord>
    {
        public GetEventsEnumerable([NotNull] ILiveRecordTicksMarker liveRecordTicksMarker,
                                   ISerializer serializer,
                                   IColumnFamilyConnection connection,
                                   long fromTicks,
                                   long toTicks,
                                   int batchSize,
                                   ILog logger)
        {
            this.liveRecordTicksMarker = liveRecordTicksMarker;
            this.serializer = serializer;
            this.connection = connection;
            this.fromTicks = fromTicks;
            this.toTicks = toTicks;
            this.batchSize = batchSize;
            this.logger = logger;
        }

        public IEnumerator<TaskIndexRecord> GetEnumerator()
        {
            return new GetEventsEnumerator(liveRecordTicksMarker, serializer, connection, fromTicks, toTicks, batchSize, logger);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private readonly ILiveRecordTicksMarker liveRecordTicksMarker;
        private readonly ISerializer serializer;
        private readonly IColumnFamilyConnection connection;
        private readonly long fromTicks;
        private readonly long toTicks;
        private readonly int batchSize;
        private readonly ILog logger;
    }
}