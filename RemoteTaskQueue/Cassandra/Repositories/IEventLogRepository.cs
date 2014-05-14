using System;
using System.Collections.Generic;

using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Cassandra.Repositories
{
    public interface IEventLogRepository
    {
        void AddEvent(string taskId, long nowTicks);
        IEnumerable<TaskMetaUpdatedEvent> GetEvents(long fromTicks, int batchSize = 2000);
        TimeSpan UnstableZoneLength { get; }
    }
}