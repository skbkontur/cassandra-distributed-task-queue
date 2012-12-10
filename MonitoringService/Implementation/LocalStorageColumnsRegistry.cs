using SKBKontur.Catalogue.Core.SQL;
using SKBKontur.Catalogue.Core.SynchronizationStorage.Indexes;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringService.Implementation
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