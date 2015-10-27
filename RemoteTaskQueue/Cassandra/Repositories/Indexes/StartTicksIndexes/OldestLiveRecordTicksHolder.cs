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

        [CanBeNull]
        public ILiveRecordTicksMarker TryGetCurrentMarkerValue([NotNull] TaskTopicAndState taskTopicAndState)
        {
            var currentMarkerValue = DoTryGetCurrentMarkerValue(taskTopicAndState);
            if(currentMarkerValue != null)
                return currentMarkerValue;
            var persistedTicks = ticksHolder.GetMinTicks(taskTopicAndState.ToCassandraKey());
            if(persistedTicks == 0)
                return null;
            return DoGetCurrentMarkerValue(taskTopicAndState, persistedTicks);
        }

        [CanBeNull]
        private ILiveRecordTicksMarker DoTryGetCurrentMarkerValue([NotNull] TaskTopicAndState taskTopicAndState)
        {
            lock(locker)
            {
                long currentTicks;
                if(!ticksByTaskState.TryGetValue(taskTopicAndState, out currentTicks))
                    return null;
                return new LiveRecordTicksMarker(taskTopicAndState, currentTicks, this);
            }
        }

        [NotNull]
        private ILiveRecordTicksMarker DoGetCurrentMarkerValue([NotNull] TaskTopicAndState taskTopicAndState, long persistedTicks)
        {
            lock(locker)
            {
                long currentTicks;
                if(!ticksByTaskState.TryGetValue(taskTopicAndState, out currentTicks))
                {
                    currentTicks = persistedTicks;
                    ticksByTaskState.Add(taskTopicAndState, currentTicks);
                }
                else
                {
                    if(persistedTicks < currentTicks)
                    {
                        currentTicks = persistedTicks;
                        ticksByTaskState[taskTopicAndState] = currentTicks;
                    }
                }
                return new LiveRecordTicksMarker(taskTopicAndState, currentTicks, this);
            }
        }

        public bool TryMoveForward([NotNull] TaskTopicAndState taskTopicAndState, long oldTicks, long newTicks)
        {
            lock(locker)
            {
                long currentTicks;
                if(!ticksByTaskState.TryGetValue(taskTopicAndState, out currentTicks))
                    throw new InvalidProgramStateException(string.Format("Not found CurrentTicks for: {0}", taskTopicAndState));
                if(currentTicks < oldTicks)
                    return false;
                ticksByTaskState[taskTopicAndState] = newTicks;
                return true;
            }
        }

        public void MoveMarkerBackwardIfNecessary([NotNull] TaskTopicAndState taskTopicAndState, long newTicks)
        {
            ticksHolder.UpdateMinTicks(taskTopicAndState.ToCassandraKey(), newTicks);
            DoMoveBackwardIfNecessary(taskTopicAndState, newTicks);
        }

        private void DoMoveBackwardIfNecessary([NotNull] TaskTopicAndState taskTopicAndState, long newTicks)
        {
            lock(locker)
            {
                long currentTicks;
                if(!ticksByTaskState.TryGetValue(taskTopicAndState, out currentTicks))
                    ticksByTaskState.Add(taskTopicAndState, newTicks);
                else
                {
                    if(newTicks < currentTicks)
                        ticksByTaskState[taskTopicAndState] = newTicks;
                }
            }
        }

        private readonly ITicksHolder ticksHolder;
        private readonly object locker = new object();
        private readonly Dictionary<TaskTopicAndState, long> ticksByTaskState = new Dictionary<TaskTopicAndState, long>();
    }
}