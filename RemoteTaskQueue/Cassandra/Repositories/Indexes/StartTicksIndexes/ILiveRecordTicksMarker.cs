using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public interface ILiveRecordTicksMarker
    {
        [NotNull]
        LiveRecordTicksMarkerState State { get; }

        void TryMoveForward(long newTicks);

        void CommitChanges();
    }
}