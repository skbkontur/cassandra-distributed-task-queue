using System.Collections.Generic;

using JetBrains.Annotations;

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

        public long? TryStartReadToEndSession([NotNull] TaskNameAndState taskNameAndState)
        {
            var ticks = DoTryStartReadToEndSession(taskNameAndState);
            if(ticks.HasValue)
                return ticks.Value;
            var persistedTicks = ticksHolder.GetMinTicks(taskNameAndState.ToCassandraKey());
            if(persistedTicks == 0)
                return null;
            return DoStartReadToEndSession(taskNameAndState, persistedTicks);
        }

        private long? DoTryStartReadToEndSession([NotNull] TaskNameAndState taskNameAndState)
        {
            lock(locker)
            {
                OldestLiveRecordTicksItem item;
                if(!ticksByTaskState.TryGetValue(taskNameAndState, out item))
                    return null;
                item.IsAllowedToMoveForward = true;
                return item.Ticks;
            }
        }

        private long DoStartReadToEndSession([NotNull] TaskNameAndState taskNameAndState, long persistedTicks)
        {
            lock(locker)
            {
                OldestLiveRecordTicksItem item;
                if(!ticksByTaskState.TryGetValue(taskNameAndState, out item))
                {
                    item = new OldestLiveRecordTicksItem {Ticks = persistedTicks};
                    ticksByTaskState.Add(taskNameAndState, item);
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

        public bool TryMoveForward([NotNull] TaskNameAndState taskNameAndState, long newTicks)
        {
            lock(locker)
            {
                OldestLiveRecordTicksItem item;
                if(!ticksByTaskState.TryGetValue(taskNameAndState, out item))
                    throw new InvalidProgramStateException(string.Format("Not found OldestLiveRecordTicksItem for: {0}", taskNameAndState));
                if(!item.IsAllowedToMoveForward)
                    return false;
                item.Ticks = newTicks;
                item.IsAllowedToMoveForward = false;
                return true;
            }
        }

        public void MoveBackwardIfNecessary([NotNull] TaskNameAndState taskNameAndState, long newTicks)
        {
            ticksHolder.UpdateMinTicks(taskNameAndState.ToCassandraKey(), newTicks);
            DoMoveBackwardIfNecessary(taskNameAndState, newTicks);
        }

        private void DoMoveBackwardIfNecessary([NotNull] TaskNameAndState taskNameAndState, long newTicks)
        {
            lock(locker)
            {
                OldestLiveRecordTicksItem item;
                if(!ticksByTaskState.TryGetValue(taskNameAndState, out item))
                {
                    item = new OldestLiveRecordTicksItem
                        {
                            Ticks = newTicks,
                            IsAllowedToMoveForward = false,
                        };
                    ticksByTaskState.Add(taskNameAndState, item);
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
        private readonly Dictionary<TaskNameAndState, OldestLiveRecordTicksItem> ticksByTaskState = new Dictionary<TaskNameAndState, OldestLiveRecordTicksItem>();

        private class OldestLiveRecordTicksItem
        {
            public long Ticks { get; set; }
            public bool IsAllowedToMoveForward { get; set; }
        }
    }
}