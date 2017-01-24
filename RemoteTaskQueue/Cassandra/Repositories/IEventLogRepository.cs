using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Cassandra.Repositories
{
    public interface IEventLogRepository
    {
        TimeSpan UnstableZoneLength { get; }

        void AddEvent([NotNull] TaskMetaInformation taskMeta, long nowTicks);

        [NotNull]
        IEnumerable<TaskMetaUpdatedEvent> GetEvents(long fromTicks, long toTicks, int batchSize);
    }
}