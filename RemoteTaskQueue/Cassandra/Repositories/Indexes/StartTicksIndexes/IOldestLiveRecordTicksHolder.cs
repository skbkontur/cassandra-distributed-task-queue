using JetBrains.Annotations;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public interface IOldestLiveRecordTicksHolder
    {
        long? TryStartReadToEndSession([NotNull] TaskNameAndState taskNameAndState);
        bool TryMoveForward([NotNull] TaskNameAndState taskNameAndState, long newTicks);
        void MoveBackwardIfNecessary([NotNull] TaskNameAndState taskNameAndState, long newTicks);
    }
}