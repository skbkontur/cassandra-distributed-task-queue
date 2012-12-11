using SKBKontur.Catalogue.Core.SynchronizationStorage.LocalStorage;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation
{
    public class LocalStorageTableRegistry : LocalStorageTableRegistryBase
    {
        public LocalStorageTableRegistry()
        {
            Register<TaskMetaInformationBusinessObjectWrap>("TaskMetaInformation");
        }
    }
}