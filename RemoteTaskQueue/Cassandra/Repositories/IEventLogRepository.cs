using System;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

using SkbKontur.Cassandra.TimeBasedUuid;

namespace RemoteQueue.Cassandra.Repositories
{
    public interface IEventLogRepository
    {
        TimeSpan UnstableZoneLength { get; }

        void AddEvent([NotNull] TaskMetaInformation taskMeta, [NotNull] Timestamp eventTimestamp, Guid eventId);
    }
}