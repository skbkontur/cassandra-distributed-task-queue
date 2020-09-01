using System;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories
{
    public interface IEventLogRepository
    {
        void AddEvent([NotNull] TaskMetaInformation taskMeta, [NotNull] Timestamp eventTimestamp, Guid eventId);

        long GetFirstEventTicks();

        [NotNull]
        ( /*[NotNull]*/ TaskMetaUpdatedEvent Event, /*[NotNull]*/ string Offset)[] GetEvents([NotNull] string partitionKey,
                                                                                             [NotNull] string exclusiveStartColumnName,
                                                                                             [NotNull] string inclusiveEndColumnName,
                                                                                             int eventsToFetch,
                                                                                             out bool currentPartitionIsExhausted);
    }
}