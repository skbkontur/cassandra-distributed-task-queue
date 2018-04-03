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
        public ILiveRecordTicksMarker TryGetCurrentMarkerValue([NotNull] TaskIndexShardKey taskIndexShardKey)
        {
            var currentMarkerValue = DoTryGetCurrentMarkerValue(taskIndexShardKey);
            if (currentMarkerValue != null)
                return currentMarkerValue;
            var persistedTicks = ticksHolder.GetMinTicks(taskIndexShardKey.ToCassandraKey());
            if (persistedTicks == 0)
                return null;
            return DoGetCurrentMarkerValue(taskIndexShardKey, persistedTicks);
        }

        [CanBeNull]
        private ILiveRecordTicksMarker DoTryGetCurrentMarkerValue([NotNull] TaskIndexShardKey taskIndexShardKey)
        {
            lock (locker)
            {
                long currentTicks;
                if (!ticksByShardKey.TryGetValue(taskIndexShardKey, out currentTicks))
                    return null;
                return new LiveRecordTicksMarker(new LiveRecordTicksMarkerState(taskIndexShardKey, currentTicks), this);
            }
        }

        [NotNull]
        private ILiveRecordTicksMarker DoGetCurrentMarkerValue([NotNull] TaskIndexShardKey taskIndexShardKey, long persistedTicks)
        {
            lock (locker)
            {
                long currentTicks;
                if (!ticksByShardKey.TryGetValue(taskIndexShardKey, out currentTicks))
                {
                    currentTicks = persistedTicks;
                    ticksByShardKey.Add(taskIndexShardKey, currentTicks);
                }
                else
                {
                    if (persistedTicks < currentTicks)
                    {
                        currentTicks = persistedTicks;
                        ticksByShardKey[taskIndexShardKey] = currentTicks;
                    }
                }
                return new LiveRecordTicksMarker(new LiveRecordTicksMarkerState(taskIndexShardKey, currentTicks), this);
            }
        }

        public bool TryMoveForward([NotNull] TaskIndexShardKey taskIndexShardKey, long oldTicks, long newTicks)
        {
            lock (locker)
            {
                long currentTicks;
                if (!ticksByShardKey.TryGetValue(taskIndexShardKey, out currentTicks))
                    throw new InvalidProgramStateException(string.Format("Not found CurrentTicks for: {0}", taskIndexShardKey));
                if (currentTicks < oldTicks)
                    return false;
                ticksByShardKey[taskIndexShardKey] = newTicks;
                return true;
            }
        }

        public void MoveMarkerBackwardIfNecessary([NotNull] TaskIndexShardKey taskIndexShardKey, long newTicks)
        {
            ticksHolder.UpdateMinTicks(taskIndexShardKey.ToCassandraKey(), newTicks);
            DoMoveBackwardIfNecessary(taskIndexShardKey, newTicks);
        }

        private void DoMoveBackwardIfNecessary([NotNull] TaskIndexShardKey taskIndexShardKey, long newTicks)
        {
            lock (locker)
            {
                long currentTicks;
                if (!ticksByShardKey.TryGetValue(taskIndexShardKey, out currentTicks))
                    ticksByShardKey.Add(taskIndexShardKey, newTicks);
                else
                {
                    if (newTicks < currentTicks)
                        ticksByShardKey[taskIndexShardKey] = newTicks;
                }
            }
        }

        private readonly ITicksHolder ticksHolder;
        private readonly object locker = new object();
        private readonly Dictionary<TaskIndexShardKey, long> ticksByShardKey = new Dictionary<TaskIndexShardKey, long>();
    }
}