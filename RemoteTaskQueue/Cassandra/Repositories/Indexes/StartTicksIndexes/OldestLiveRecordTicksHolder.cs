using System.Collections.Generic;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public class OldestLiveRecordTicksHolder : IOldestLiveRecordTicksHolder
    {
        public OldestLiveRecordTicksHolder(ITicksHolder ticksHolder)
        {
            this.ticksHolder = ticksHolder;
        }

        public long? TryStartReadToEndSession(TaskState taskState)
        {
            var ticks = DoTryStartReadToEndSession(taskState);
            if(ticks.HasValue)
                return ticks.Value;
            var persistedTicks = ticksHolder.GetMinTicks(taskState.GetCassandraName());
            if(persistedTicks == 0)
                return null;
            return DoStartReadToEndSession(taskState, persistedTicks);
        }

        private long? DoTryStartReadToEndSession(TaskState taskState)
        {
            lock(locker)
            {
                OldestLiveRecordTicksItem item;
                if(!ticksByTaskState.TryGetValue(taskState, out item))
                    return null;
                item.IsAllowedToMoveForward = true;
                return item.Ticks;
            }
        }

        private long DoStartReadToEndSession(TaskState taskState, long persistedTicks)
        {
            lock(locker)
            {
                OldestLiveRecordTicksItem item;
                if(!ticksByTaskState.TryGetValue(taskState, out item))
                {
                    item = new OldestLiveRecordTicksItem {Ticks = persistedTicks};
                    ticksByTaskState.Add(taskState, item);
                }
                else
                {
                    if(persistedTicks < item.Ticks)
                        item.Ticks = persistedTicks;
                }
                item.IsAllowedToMoveForward = true;
                return item.Ticks;
            }
        }

        public void TryMoveForward(TaskState taskState, long newTicks)
        {
            lock(locker)
            {
                OldestLiveRecordTicksItem item;
                if(!ticksByTaskState.TryGetValue(taskState, out item))
                    throw new InvalidProgramStateException(string.Format("Not found OldestLiveRecordTicksItem for: {0}", taskState));
                if(item.IsAllowedToMoveForward)
                {
                    item.Ticks = newTicks;
                    item.IsAllowedToMoveForward = false;
                }
            }
        }

        public void MoveBackwardIfNecessary(TaskState taskState, long newTicks)
        {
            ticksHolder.UpdateMinTicks(taskState.GetCassandraName(), newTicks);
            DoMoveBackwardIfNecessary(taskState, newTicks);
        }

        private void DoMoveBackwardIfNecessary(TaskState taskState, long newTicks)
        {
            lock(locker)
            {
                OldestLiveRecordTicksItem item;
                if(!ticksByTaskState.TryGetValue(taskState, out item))
                {
                    item = new OldestLiveRecordTicksItem
                        {
                            Ticks = newTicks,
                            IsAllowedToMoveForward = false,
                        };
                    ticksByTaskState.Add(taskState, item);
                }
                else
                {
                    if(newTicks < item.Ticks)
                    {
                        item.Ticks = newTicks;
                        item.IsAllowedToMoveForward = false;
                    }
                }
            }
        }

        private readonly ITicksHolder ticksHolder;
        private readonly object locker = new object();
        private readonly Dictionary<TaskState, OldestLiveRecordTicksItem> ticksByTaskState = new Dictionary<TaskState, OldestLiveRecordTicksItem>();

        private class OldestLiveRecordTicksItem
        {
            public long Ticks { get; set; }
            public bool IsAllowedToMoveForward { get; set; }
        }
    }
}