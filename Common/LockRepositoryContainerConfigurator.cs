using GroboContainer.Core;

using GroBuf;

using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock;
using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock.RemoteLocker;
using SKBKontur.Catalogue.CassandraPrimitives.Storages.Primitives;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Common
{
    public static class LockRepositoryContainerConfigurator
    {
        public static void ConfigureLockRepository(this IContainer container, string remoteLockColumnFamily = null)
        {
            var serializer = container.Get<ISerializer>();
            var cassandraCluster = container.Get<ICassandraCluster>();
            var taskQueueSettings = container.Get<IRemoteTaskQueueSettings>();
            remoteLockColumnFamily = remoteLockColumnFamily ?? RemoteTaskQueueLockConstants.LockColumnFamily;
            var cassandraRemoteLockImplementationSettings = CassandraRemoteLockImplementationSettings.Default(new ColumnFamilyFullName(taskQueueSettings.QueueKeyspaceForLock, remoteLockColumnFamily));
            var remoteLockImplementation = new CassandraRemoteLockImplementation(cassandraCluster, serializer, cassandraRemoteLockImplementationSettings);
            container.Configurator.ForAbstraction<IRemoteLockCreator>().UseInstances(new RemoteLocker(remoteLockImplementation, new RemoteLockerMetrics(taskQueueSettings.QueueKeyspace)));
        }
    }
}