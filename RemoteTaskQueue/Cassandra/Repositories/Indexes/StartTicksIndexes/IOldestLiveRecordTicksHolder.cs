using JetBrains.Annotations;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public interface IOldestLiveRecordTicksHolder
    {
        [CanBeNull]
        ILiveRecordTicksMarker TryGetCurrentMarkerValue([NotNull] TaskIndexShardKey taskIndexShardKey);

        void MoveMarkerBackwardIfNecessary([NotNull] TaskIndexShardKey taskIndexShardKey, long newTicks);
    }
}