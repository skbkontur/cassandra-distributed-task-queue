using GroBuf;

using GroboContainer.Core;


using RemoteQueue.Cassandra.Primitives;

using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock;
using SKBKontur.Catalogue.CassandraPrimitives.Storages.Primitives;

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
                    serializer,
                    new ColumnFamilyFullName(columnFamilyRepositoryParameters.Settings.QueueKeyspace,
                    columnFamilyRepositoryParameters.LockColumnFamilyName)));
        }
    }
}