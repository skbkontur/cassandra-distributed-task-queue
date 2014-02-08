using System.Collections.Generic;

using RemoteQueue.Cassandra.Entities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation
{
    public interface IProcessedEvents
    {
        void AddEvents(IEnumerable<TaskMetaUpdatedEvent> events);
        void RemoveEvents(long threshold);
        bool Contains(TaskMetaUpdatedEvent elmentaryEvent);
        int GetCount();
        void Clear();
    }
}