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
            var oldestLiveRecordTicks = DoTryStartReadToEndSession(taskState);
            if(oldestLiveRecordTicks.HasValue)
                return oldestLiveRecordTicks.Value;
            var persistedOldestLiveRecordTicks = ticksHolder.GetMinTicks(taskState.GetCassandraName());
            if(persistedOldestLiveRecordTicks == 0)
                return null;
            return StartReadToEndSession(taskState, persistedOldestLiveRecordTicks);
        }

        private long? DoTryStartReadToEndSession(TaskState taskState)
        {
            lock(locker)
            {
                OldestLiveRecordTicksItem item;
                if(!ticksByTaskState.TryGetValue(taskState, out item))
                    return null;
                item.IsAllowedToMoveForward = true;
                return item.OldestLiveRecordTicks;
            }
        }

        private long StartReadToEndSession(TaskState taskState, long persistedOldestLiveRecordTicks)
        {
            lock(locker)
            {
                OldestLiveRecordTicksItem item;
                if(!ticksByTaskState.TryGetValue(taskState, out item))
                {
                    item = new OldestLiveRecordTicksItem {OldestLiveRecordTicks = persistedOldestLiveRecordTicks};
                    ticksByTaskState.Add(taskState, item);
                }
                else
                {
                    if(persistedOldestLiveRecordTicks < item.OldestLiveRecordTicks)
                        item.OldestLiveRecordTicks = persistedOldestLiveRecordTicks;
                }
                item.IsAllowedToMoveForward = true;
                return item.OldestLiveRecordTicks;
            }
        }

        public void TryMoveForward(TaskState taskState, long newOldestLiveRecordTicks)
        {
            lock(locker)
            {
                OldestLiveRecordTicksItem item;
                if(!ticksByTaskState.TryGetValue(taskState, out item))
                    throw new InvalidProgramStateException(string.Format("Not found OldestLiveRecordTicksItem for: {0}", taskState));
                if(item.IsAllowedToMoveForward)
                {
                    item.OldestLiveRecordTicks = newOldestLiveRecordTicks;
                    item.IsAllowedToMoveForward = false;
                }
            }
        }

        public void MoveBackwardIfNecessary(TaskState taskState, long newOldestLiveRecordTicks)
        {
            ticksHolder.UpdateMinTicks(taskState.GetCassandraName(), newOldestLiveRecordTicks);
            DoMoveBackwardIfNecessary(taskState, newOldestLiveRecordTicks);
        }

        private void DoMoveBackwardIfNecessary(TaskState taskState, long newOldestLiveRecordTicks)
        {
            lock(locker)
            {
                OldestLiveRecordTicksItem item;
                if(!ticksByTaskState.TryGetValue(taskState, out item))
                {
                    item = new OldestLiveRecordTicksItem
                        {
                            OldestLiveRecordTicks = newOldestLiveRecordTicks,
                            IsAllowedToMoveForward = false,
                        };
                    ticksByTaskState.Add(taskState, item);
                }
                else
                {
                    if(newOldestLiveRecordTicks < item.OldestLiveRecordTicks)
                    {
                        item.OldestLiveRecordTicks = newOldestLiveRecordTicks;
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
            public long OldestLiveRecordTicks { get; set; }
            public bool IsAllowedToMoveForward { get; set; }
        }
    }
}