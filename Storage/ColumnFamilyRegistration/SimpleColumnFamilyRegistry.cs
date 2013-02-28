using RemoteQueue.Cassandra.Primitives;

using SKBKontur.Catalogue.CassandraStorageCore;
using SKBKontur.Catalogue.CassandraStorageCore.ColumnFamilyRegistration;
using SKBKontur.Catalogue.CassandraStorageCore.EventLog;
using SKBKontur.Catalogue.CassandraStorageCore.FileDataStorage;
using SKBKontur.Catalogue.CassandraStorageCore.GlobalTicks;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Storage.ColumnFamilyRegistration
{
    public sealed class SimpleColumnFamilyRegistry : AbstractColumnFamilyRegistry, ISimpleColumnFamilyRegistry
    {
        public SimpleColumnFamilyRegistry(
            ICassandraCoreSettings cassandraCoreSettings)
            : base(cassandraCoreSettings)
        {
            RegisterCommon(GlobalTicksHolder.ColumnFamilyName);

            RegisterLocal(EventLogRepository.LocalColumnFamilyName);
            RegisterLocal(FileDataStorage.ColumnFamilyName);
            RegisterLocal(EventLogRepository.CommonColumnFamilyName);
            RegisterLocal(ColumnFamilyRepositoryParameters.LockColumnFamily);
            RegisterLocal(RemoteLockConfigurator.RemoteLockColumnFamily);
        }
    }
}