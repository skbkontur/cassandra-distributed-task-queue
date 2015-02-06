using GroboContainer.Core;

using RemoteQueue.Cassandra.Primitives;

using SKBKontur.Catalogue.CassandraStorageCore;
using SKBKontur.Catalogue.CassandraStorageCore.Storage.BusinessObjects.Schema;
using SKBKontur.Catalogue.CassandraStorageCore.Storage.Persistent.Cassandra.Schema;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Storage
{
    public class RemoteTaskQueueMonitoringSchemaConfiguration : StorageSchemaConfigurator
    {
        public RemoteTaskQueueMonitoringSchemaConfiguration(ICassandraCoreSettings cassandraCoreSettings)
            : base(cassandraCoreSettings.KeyspaceName)
        {
        }

        protected override void ConfigureBusinessObjectSchema(BusinessObjectStoringSchema schema)
        {
            schema.DefineType<MonitoringSearchRequest>(c => c.TypeIdentifier("MonitoringSearchRequest"));
        }

        protected override void DoConfigureCassandraBasicSchema(CassandraStoringSchema schema)
        {
            schema.ColumnFamily("lock", c => c.Name(ColumnFamilyRepositoryParameters.LockColumnFamily).KeyspaceName(LocalKeyspaceName));
        }

        protected override void ConfigureCassandraBusinessObjects(CassandraStoringSchema schema, IContainer container)
        {
        }
    }
}