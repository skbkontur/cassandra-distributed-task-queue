using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.Cassandra.Repositories
{
    public interface IEventLogRepository
    {
        TimeSpan UnstableZoneLength { get; }

        void AddEvent([NotNull] TaskMetaInformation taskMeta, [NotNull] Timestamp eventTimestamp, Guid eventId);

        [NotNull]
        IEnumerable<TaskMetaUpdatedEvent> GetEvents(long fromTicks, long toTicks, int batchSize);
    }
}