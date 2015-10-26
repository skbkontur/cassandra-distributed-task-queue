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

        public long? TryStartReadToEndSession([NotNull] TaskTopicAndState taskTopicAndState)
        {
            var ticks = DoTryStartReadToEndSession(taskTopicAndState);
            if(ticks.HasValue)
                return ticks.Value;
            var persistedTicks = ticksHolder.GetMinTicks(taskTopicAndState.ToCassandraKey());
            if(persistedTicks == 0)
                return null;
            return DoStartReadToEndSession(taskTopicAndState, persistedTicks);
        }

        private long? DoTryStartReadToEndSession([NotNull] TaskTopicAndState taskTopicAndState)
        {
            lock(locker)
            {
                OldestLiveRecordTicksItem item;
                if(!ticksByTaskState.TryGetValue(taskTopicAndState, out item))
                    return null;
                item.IsAllowedToMoveForward = true;
                return item.Ticks;
            }
        }

        private long DoStartReadToEndSession([NotNull] TaskTopicAndState taskTopicAndState, long persistedTicks)
        {
            lock(locker)
            {
                OldestLiveRecordTicksItem item;
                if(!ticksByTaskState.TryGetValue(taskTopicAndState, out item))
                {
                    item = new OldestLiveRecordTicksItem {Ticks = persistedTicks};
                    ticksByTaskState.Add(taskTopicAndState, item);
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

        public bool TryMoveForward([NotNull] TaskTopicAndState taskTopicAndState, long newTicks)
        {
            lock(locker)
            {
                OldestLiveRecordTicksItem item;
                if(!ticksByTaskState.TryGetValue(taskTopicAndState, out item))
                    throw new InvalidProgramStateException(string.Format("Not found OldestLiveRecordTicksItem for: {0}", taskTopicAndState));
                if(!item.IsAllowedToMoveForward)
                    return false;
                item.Ticks = newTicks;
                item.IsAllowedToMoveForward = false;
                return true;
            }
        }

        public void MoveBackwardIfNecessary([NotNull] TaskTopicAndState taskTopicAndState, long newTicks)
        {
            ticksHolder.UpdateMinTicks(taskTopicAndState.ToCassandraKey(), newTicks);
            DoMoveBackwardIfNecessary(taskTopicAndState, newTicks);
        }

        private void DoMoveBackwardIfNecessary([NotNull] TaskTopicAndState taskTopicAndState, long newTicks)
        {
            lock(locker)
            {
                OldestLiveRecordTicksItem item;
                if(!ticksByTaskState.TryGetValue(taskTopicAndState, out item))
                {
                    item = new OldestLiveRecordTicksItem
                        {
                            Ticks = newTicks,
                            IsAllowedToMoveForward = false,
                        };
                    ticksByTaskState.Add(taskTopicAndState, item);
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
        private readonly Dictionary<TaskTopicAndState, OldestLiveRecordTicksItem> ticksByTaskState = new Dictionary<TaskTopicAndState, OldestLiveRecordTicksItem>();

        private class OldestLiveRecordTicksItem
        {
            public long Ticks { get; set; }
            public bool IsAllowedToMoveForward { get; set; }
        }
    }
}