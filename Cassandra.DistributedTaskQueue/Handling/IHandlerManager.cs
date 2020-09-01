using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
{
    internal interface IHandlerManager
    {
        [NotNull]
        string JobId { get; }

        void RunJobIteration();

        [NotNull]
        LiveRecordTicksMarkerState[] GetCurrentLiveRecordTicksMarkers();
    }
}