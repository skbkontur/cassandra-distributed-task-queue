using JetBrains.Annotations;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public interface IOldestLiveRecordTicksHolder
    {
        long? TryStartReadToEndSession([NotNull] TaskTopicAndState taskTopicAndState);
        bool TryMoveForward([NotNull] TaskTopicAndState taskTopicAndState, long newTicks);
        void MoveBackwardIfNecessary([NotNull] TaskTopicAndState taskTopicAndState, long newTicks);
    }
}