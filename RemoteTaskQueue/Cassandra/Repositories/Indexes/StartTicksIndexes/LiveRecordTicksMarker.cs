using JetBrains.Annotations;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public class LiveRecordTicksMarker : ILiveRecordTicksMarker
    {
        public LiveRecordTicksMarker([NotNull] LiveRecordTicksMarkerState state, [NotNull] OldestLiveRecordTicksHolder ticksHolder)
        {
            State = state;
            this.ticksHolder = ticksHolder;
        }

        [NotNull]
        public LiveRecordTicksMarkerState State { get; private set; }

        public void TryMoveForward(long newTicks)
        {
            if (!minimalNewTicks.HasValue || newTicks < minimalNewTicks.Value)
                minimalNewTicks = newTicks;
        }

        public void CommitChanges()
        {
            if (minimalNewTicks.HasValue)
                ticksHolder.TryMoveForward(State.TaskIndexShardKey, State.CurrentTicks, minimalNewTicks.Value);
        }

        private long? minimalNewTicks;
        private readonly OldestLiveRecordTicksHolder ticksHolder;
    }
}