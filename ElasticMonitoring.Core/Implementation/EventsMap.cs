using System.Collections.Generic;
using System.Linq;
using System.Threading;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.Utils;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation
{
    public class EventsMap
    {
        public EventsMap(long cacheEventTicks)
        {
            this.cacheEventTicks = cacheEventTicks;
        }

        public long? GetOldestEventTime()
        {
            if(map.Count <= 0)
                return null;
            var min = map.Min(pair => pair.Value.Ticks);
            return min;
        }

        public bool NotContains(TaskMetaUpdatedEvent e)
        {
            TaskMetaUpdatedEvent existingEvent;
            if(map.TryGetValue(e.TaskId, out existingEvent))
            {
                if(existingEvent.Ticks >= e.Ticks)
                    return false;
            }
            return true;
        }

        public void CollectGarbage(long nowTicks)
        {
            map.DeleteWhere(pair => pair.Value.Ticks + cacheEventTicks < nowTicks);
            UpdateCount();
        }

        public IEnumerable<TaskMetaUpdatedEvent> GetEvents()
        {
            return map.Values.ToArray();
        }

        public void Clear()
        {
            map.Clear();
            UpdateCount();
        }

        public void AddEvent(TaskMetaUpdatedEvent e)
        {
            TaskMetaUpdatedEvent existingEvent;
            if(map.TryGetValue(e.TaskId, out existingEvent))
            {
                if(existingEvent.Ticks >= e.Ticks)
                    return;
            }
            map[e.TaskId] = e;
            UpdateCount();
        }

        public void RemoveEvent(TaskMetaUpdatedEvent e)
        {
            TaskMetaUpdatedEvent existingEvent;
            if(map.TryGetValue(e.TaskId, out existingEvent))
            {
                if(existingEvent.Ticks > e.Ticks)
                    return;
            }
            map.Remove(e.TaskId);
            UpdateCount();
        }

        private void UpdateCount()
        {
            Interlocked.Exchange(ref count, map.Count);
        }

        public long GetUnsafeCount()
        {
            return Interlocked.Read(ref count);
        }

        private long count;
        private readonly Dictionary<string, TaskMetaUpdatedEvent> map = new Dictionary<string, TaskMetaUpdatedEvent>();
        private readonly long cacheEventTicks;
    }
}