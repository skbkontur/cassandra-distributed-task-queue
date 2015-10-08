using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public interface IOldestLiveRecordTicksHolder
    {
        long? TryStartReadToEndSession(TaskState taskState);
        bool TryMoveForward(TaskState taskState, long newTicks);
        void MoveBackwardIfNecessary(TaskState taskState, long newTicks);
    }
}