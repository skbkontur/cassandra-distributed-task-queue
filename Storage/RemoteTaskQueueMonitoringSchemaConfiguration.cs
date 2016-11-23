using GroboContainer.Core;

using SKBKontur.Catalogue.CassandraPrimitives.Storages.Primitives;
using SKBKontur.Catalogue.CassandraStorageCore;
using SKBKontur.Catalogue.CassandraStorageCore.EventLog;
using SKBKontur.Catalogue.CassandraStorageCore.FileDataStorage;
using SKBKontur.Catalogue.CassandraStorageCore.GlobalTicks;
using SKBKontur.Catalogue.CassandraStorageCore.RemoteLock;
using SKBKontur.Catalogue.CassandraStorageCore.Storage.BusinessObjects.Schema;
using SKBKontur.Catalogue.CassandraStorageCore.Storage.Persistent.Cassandra.Schema;

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
        }

        protected override sealed void ConfigureCassandraBasicSchema(CassandraStoringSchema schema)
        {
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