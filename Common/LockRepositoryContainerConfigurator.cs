using GroboContainer.Core;

using RemoteLock;

using RemoteQueue.Cassandra.Primitives;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Common
{
    public static class LockRepositoryContainerConfigurator
    {
        public static void ConfigureLockRepository(this IContainer container)
        {
            var columnFamilyRepositoryParameters = container.Get<IColumnFamilyRepositoryParameters>();
            container.Configurator.ForAbstraction<ILockRepository>().UseInstances(
                new LockRepository(
                    columnFamilyRepositoryParameters.CassandraCluster,
                    columnFamilyRepositoryParameters.Settings.QueueKeyspace,
                    columnFamilyRepositoryParameters.LockColumnFamilyName));
        }
    }
}