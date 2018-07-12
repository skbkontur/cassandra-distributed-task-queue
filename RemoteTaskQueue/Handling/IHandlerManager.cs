using JetBrains.Annotations;

using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;

using SKBKontur.Catalogue.ServiceLib.Scheduling;

namespace RemoteQueue.Handling
{
    public interface IHandlerManager : IPeriodicTask
    {
        [NotNull]
        LiveRecordTicksMarkerState[] GetCurrentLiveRecordTicksMarkers();
    }
}