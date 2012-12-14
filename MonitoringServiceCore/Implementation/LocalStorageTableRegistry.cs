using SKBKontur.Catalogue.Core.SynchronizationStorage.LocalStorage;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation
{
    public class LocalStorageTableRegistry : LocalStorageTableRegistryBase
    {
        public LocalStorageTableRegistry()
        {
            Register<MonitoringTaskMetadata>("MonitoringTaskMetadata");
        }
    }
}