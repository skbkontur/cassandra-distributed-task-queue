using GroboContainer.Core;

using RemoteQueue.Cassandra.Primitives;

using SKBKontur.Catalogue.CassandraStorageCore;
using SKBKontur.Catalogue.CassandraStorageCore.EventLog;
using SKBKontur.Catalogue.CassandraStorageCore.FileDataStorage;
using SKBKontur.Catalogue.CassandraStorageCore.GlobalTicks;
using SKBKontur.Catalogue.CassandraStorageCore.RemoteLock;
using SKBKontur.Catalogue.CassandraStorageCore.Storage.BusinessObjects.Schema;
using SKBKontur.Catalogue.CassandraStorageCore.Storage.Persistent;
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

        protected override sealed ColumnFamilyFullName GetDefaultBosEventLogColumnFamilyFullName()
        {
            return new ColumnFamilyFullName(LocalKeyspaceName, bosEventLogColumnFamilyName);
        }

        protected override void ConfigureBusinessObjectSchema(BusinessObjectStoringSchema schema)
        {
            schema.DefineType<MonitoringSearchRequest>(c => c.TypeIdentifier("MonitoringSearchRequest"));
        }

        protected override sealed void ConfigureCassandraBasicSchema(CassandraStoringSchema schema)
        {
            RemoteLockConfiguration.ConfigureRemoteLock(schema, LocalKeyspaceName, RemoteLockConfiguration.RemoteLockColumnFamily);
            RemoteLockConfiguration.ConfigureRemoteLock(schema, RemoteLockConfiguration.AllRemoteLocksKeyspace, RemoteLockConfiguration.CoreRemoteLockColumnFamily);
            FileDataStorageConfiguration.ConfigureCassandraSchema(schema, LocalKeyspaceName);
            GlobalTicksHolderConfiguration.ConfigureCassandraSchema(schema, LocalKeyspaceName);
            StrictBosEventLogStorageConfiguration.ConfigureCassandraSchema(schema, LocalKeyspaceName);
            schema.ColumnFamily(bosEventLogColumnFamilyName, x => x.Name(bosEventLogColumnFamilyName).KeyspaceName(LocalKeyspaceName));
        }

        protected override void ConfigureCassandraBusinessObjects(CassandraStoringSchema schema, IContainer container)
        {
        }

        private const string bosEventLogColumnFamilyName = "BosEventLog";
    }
}