using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.Core.SynchronizationStorage.LocalStorage;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringService.Implementation
{
    public class LocalStorageTableRegistry : LocalStorageTableRegistryBase
    {
        public LocalStorageTableRegistry()
        {
            Register<TaskMetaInformationBusinessObjectWrap>("TaskMetaInformation");
        }
    }
}