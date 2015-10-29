using JetBrains.Annotations;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public class LiveRecordTicksMarkerState
    {
        public LiveRecordTicksMarkerState([NotNull] TaskTopicAndState taskTopicAndState, long currentTicks)
        {
            TaskTopicAndState = taskTopicAndState;
            CurrentTicks = currentTicks;
        }

        [NotNull]
        public TaskTopicAndState TaskTopicAndState { get; private set; }

        public long CurrentTicks { get; private set; }
    }
}