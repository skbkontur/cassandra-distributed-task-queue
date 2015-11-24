using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public interface IMinTicksCache
    {
        void UpdateMinTicks(TaskState taskState, long ticks);
        long GetMinTicks(TaskState taskState);
    }
}