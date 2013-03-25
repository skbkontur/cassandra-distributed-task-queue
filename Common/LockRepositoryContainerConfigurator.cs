using GroBuf;

using GroboContainer.Core;

using RemoteLock;

using RemoteQueue.Cassandra.Primitives;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Common
{
    public static class LockRepositoryContainerConfigurator
    {
        public static void ConfigureLockRepository(this IContainer container)
        {
            var serializer = container.Get<ISerializer>();
            var columnFamilyRepositoryParameters = container.Get<IColumnFamilyRepositoryParameters>();
            container.Configurator.ForAbstraction<IRemoteLockImplementation>().UseInstances(
                new CassandraRemoteLockImplementation(
                    columnFamilyRepositoryParameters.CassandraCluster,
                    columnFamilyRepositoryParameters.Settings,
                    serializer,
                    columnFamilyRepositoryParameters.Settings.QueueKeyspace,
                    columnFamilyRepositoryParameters.LockColumnFamilyName))
                ;
        }
    }
}