using SKBKontur.Catalogue.Core.SQL;
using SKBKontur.Catalogue.Core.SynchronizationStorage.Indexes;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient.MonitoringEntities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation
{
    public class LocalStorageColumnsRegistry : LocalStorageColumnsRegistryBase
    {
        public LocalStorageColumnsRegistry(ISqlDataTypeMapper sqlDataTypeMapper, IPropertiesExtracter propertiesExtracter)
            : base(sqlDataTypeMapper, propertiesExtracter)
        {
            Register<TaskMetaInformationBusinessObjectWrap>(x => x.AllProperties());
        }
    }
}