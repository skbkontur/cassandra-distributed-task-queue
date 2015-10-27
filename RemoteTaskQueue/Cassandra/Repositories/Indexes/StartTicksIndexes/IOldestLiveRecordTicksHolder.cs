using JetBrains.Annotations;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public interface IOldestLiveRecordTicksHolder
    {
        [CanBeNull]
        ILiveRecordTicksMarker TryGetCurrentMarkerValue([NotNull] TaskTopicAndState taskTopicAndState);

        void MoveMarkerBackwardIfNecessary([NotNull] TaskTopicAndState taskTopicAndState, long newTicks);
    }
}