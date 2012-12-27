using System.Collections.Generic;

using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Cassandra.Repositories
{
    public interface IEventLogRepository
    {
        void AddEvent(TaskMetaUpdatedEvent taskMetaUpdatedEventEntity);
        IEnumerable<TaskMetaUpdatedEvent> GetEvents(long fromTicks, int batchSize = 2000);
    }
}