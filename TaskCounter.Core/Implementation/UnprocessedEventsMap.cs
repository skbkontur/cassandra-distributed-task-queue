using System.Collections.Generic;
using System.Linq;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation.Utils;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation
{
    public class UnprocessedEventsMap
    {
        public UnprocessedEventsMap(long cacheEventTicks)
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

        private void CollectOldEventsGarbage(long nowTicks)
        {
            map.DeleteWhere(pair => pair.Value.Ticks + cacheEventTicks < nowTicks);
        }

        public IEnumerable<TaskMetaUpdatedEvent> GetUnprocessedEvents(long nowTicks)
        {
            CollectOldEventsGarbage(nowTicks);
            return map.Values.ToArray();
        }

        public void Clear()
        {
            map.Clear();
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
        }

        private readonly Dictionary<string, TaskMetaUpdatedEvent> map = new Dictionary<string, TaskMetaUpdatedEvent>();
        private readonly long cacheEventTicks;
    }
}