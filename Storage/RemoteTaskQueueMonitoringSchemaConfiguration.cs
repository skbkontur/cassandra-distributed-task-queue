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
using SKBKontur.Catalogue.Core.CommonBusinessObjects.Parties;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Storage
{
    public class RemoteTaskQueueMonitoringSchemaConfiguration : StorageSchemeConfigurator
    {
        public RemoteTaskQueueMonitoringSchemaConfiguration(ICassandraCoreSettings cassandraCoreSettings)
            : base(cassandraCoreSettings.KeyspaceName)
        {
        }

        protected override void ConfigureBusinessObjectScheme(BusinessObjectStoringSchema schema)
        {
            schema.DefineType<Party>();
            schema.DefineType<PartySynonym>();
            schema.DefineType<UserStorageElement>();
            schema.DefineType<AccessRuleStorageElement>();
            schema.DefineType<FtpUser>();
            schema.DefineType<MonitoringSearchRequest>();

            schema.DefineType<UserLoginRecord>();
            schema.DefineType<ActualUserId>();
        }

        protected override void ConfigureCassandraBasicSchema(CassandraStoringSchema schema)
        {
            FileDataStorageConfiguration.ConfigureCassandraSchema(schema, LocalKeyspaceName);
            EventLogStorageConfiguration.ConfigureCassandraSchema(schema, LocalKeyspaceName);
            GlobalTicksHolderConfiguration.ConfigureCassandraSchema(schema);
            RemoteLockConfigurator.ConfigureCassandraSchema(schema, LocalKeyspaceName);
            schema.ColumnFamily("lock", c => c.Name(ColumnFamilyRepositoryParameters.LockColumnFamily).KeyspaceName(LocalKeyspaceName));
        }

        protected override void ConfigureCassandraBusinessObjects(CassandraStoringSchema schema, IContainer container)
        {
            
        }
    }
}