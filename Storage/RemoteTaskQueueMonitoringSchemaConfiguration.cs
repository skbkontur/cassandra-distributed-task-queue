using GroboContainer.Core;

using RemoteQueue.Cassandra.Primitives;

using SKBKontur.Catalogue.AccessControl;
using SKBKontur.Catalogue.AccessControl.LocalImplementation;
using SKBKontur.Catalogue.CassandraStorageCore;
using SKBKontur.Catalogue.CassandraStorageCore.EventLog;
using SKBKontur.Catalogue.CassandraStorageCore.FileDataStorage;
using SKBKontur.Catalogue.CassandraStorageCore.GlobalTicks;
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
            schema.DefineType<UserStorageElement>(c => c.TypeIdentifier("UserStorageElement"));
            schema.DefineType<AccessRuleStorageElement>(c => c.TypeIdentifier("AccessRuleStorageElement"));
            schema.DefineType<MonitoringSearchRequest>(c => c.TypeIdentifier("MonitoringSearchRequest"));

            schema.DefineType<UserLoginRecord>(c => c.TypeIdentifier("UserLoginRecord"));
            schema.DefineType<ActualUserId>(c => c.TypeIdentifier("ActualUserId"));
        }

        protected override void ConfigureCassandraBasicSchema(CassandraStoringSchema schema)
        {
            base.ConfigureCassandraBasicSchema(schema);
            FileDataStorageConfiguration.ConfigureCassandraSchema(schema, LocalKeyspaceName);
            EventLogStorageConfiguration.ConfigureCassandraSchema(schema, LocalKeyspaceName);
            GlobalTicksHolderConfiguration.ConfigureCassandraSchema(schema, LocalKeyspaceName);
            schema.ColumnFamily("lock", c => c.Name(ColumnFamilyRepositoryParameters.LockColumnFamily).KeyspaceName(LocalKeyspaceName));
        }

        protected override void ConfigureCassandraBusinessObjects(CassandraStoringSchema schema, IContainer container)
        {
        }
    }
}