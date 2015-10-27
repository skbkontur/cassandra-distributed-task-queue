using JetBrains.Annotations;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public interface ILiveRecordTicksMarker
    {
        [NotNull]
        TaskTopicAndState TaskTopicAndState { get; }

        long CurrentTicks { get; }

        void TryMoveForward(long newTicks);

        void CommitChanges();
    }
}