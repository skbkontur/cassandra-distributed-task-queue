using SKBKontur.Catalogue.Core.SQL;
using SKBKontur.Catalogue.Core.SynchronizationStorage.Indexes;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation
{
    public class LocalStorageColumnsRegistry : LocalStorageColumnsRegistryBase
    {
        public LocalStorageColumnsRegistry(ISqlDataTypeMapper sqlDataTypeMapper, IPropertiesExtracter propertiesExtracter)
            : base(sqlDataTypeMapper, propertiesExtracter)
        {
            Register<MonitoringTaskMetadata>(x => x.AllProperties());
        }
    }
}