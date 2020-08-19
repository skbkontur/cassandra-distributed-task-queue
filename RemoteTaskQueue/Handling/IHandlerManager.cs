using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using SkbKontur.Cassandra.DistributedTaskQueue.Scheduling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
{
    internal interface IHandlerManager : IPeriodicTask
    {
        [NotNull]
        LiveRecordTicksMarkerState[] GetCurrentLiveRecordTicksMarkers();
    }
}