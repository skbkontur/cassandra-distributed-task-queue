using JetBrains.Annotations;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public class LiveRecordTicksMarker : ILiveRecordTicksMarker
    {
        public LiveRecordTicksMarker([NotNull] TaskTopicAndState taskTopicAndState, long currentTicks, [NotNull] OldestLiveRecordTicksHolder ticksHolder)
        {
            this.ticksHolder = ticksHolder;
            TaskTopicAndState = taskTopicAndState;
            CurrentTicks = currentTicks;
        }

        [NotNull]
        public TaskTopicAndState TaskTopicAndState { get; private set; }

        public long CurrentTicks { get; private set; }

        public void TryMoveForward(long newTicks)
        {
            if(!minimalNewTicks.HasValue || newTicks < minimalNewTicks.Value)
                minimalNewTicks = newTicks;
        }

        public void CommitChanges()
        {
            if(minimalNewTicks.HasValue)
                ticksHolder.TryMoveForward(TaskTopicAndState, CurrentTicks, minimalNewTicks.Value);
        }

        private long? minimalNewTicks;
        private readonly OldestLiveRecordTicksHolder ticksHolder;
    }
}