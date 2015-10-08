using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public interface IFromTicksProvider
    {
        long? TryGetFromTicks(TaskState taskState);
        void HandleTaskStateChange([NotNull] TaskMetaInformation taskMeta);
        void TryUpdateOldestLiveRecordTicks(TaskState taskState, long newOldestLiveRecordTicks);
    }
}