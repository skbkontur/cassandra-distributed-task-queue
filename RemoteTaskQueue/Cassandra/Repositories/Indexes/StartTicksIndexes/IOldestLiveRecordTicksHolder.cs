using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public interface IOldestLiveRecordTicksHolder
    {
        [CanBeNull]
        ILiveRecordTicksMarker TryGetCurrentMarkerValue([NotNull] TaskIndexShardKey taskIndexShardKey);

        void MoveMarkerBackwardIfNecessary([NotNull] TaskIndexShardKey taskIndexShardKey, long newTicks);
    }
}