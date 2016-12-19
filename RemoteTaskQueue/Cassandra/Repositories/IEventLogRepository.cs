using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Cassandra.Repositories
{
    public interface IEventLogRepository
    {
        void AddEvent([NotNull] TaskMetaInformation taskMeta, long nowTicks);
        IEnumerable<TaskMetaUpdatedEvent> GetEvents(long fromTicks, long toTicks, int batchSize);
        TimeSpan UnstableZoneLength { get; }
    }
}