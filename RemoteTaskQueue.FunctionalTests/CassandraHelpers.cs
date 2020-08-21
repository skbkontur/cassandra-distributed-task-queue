using GroboContainer.Core;

using SkbKontur.Cassandra.ThriftClient.Abstractions;
using SkbKontur.Cassandra.ThriftClient.Clusters;
using SkbKontur.Cassandra.ThriftClient.Schema;

using Vostok.Logging.Abstractions;

namespace RemoteTaskQueue.FunctionalTests
{
    public static class CassandraHelpers
    {
        public static void ResetCassandraState(this IContainer container, string keyspaceName, ColumnFamily[] columnFamilies)
        {
            var logger = container.Get<ILog>();
            var cassandraCluster = container.Get<ICassandraCluster>();
            var actualizer = new CassandraSchemaActualizer(cassandraCluster, eventListener : null, logger);
            actualizer.ActualizeKeyspaces(new[]
                {
                    new KeyspaceSchema
                        {
                            Name = keyspaceName,
                            Configuration =
                                {
                                    ReplicationStrategy = SimpleReplicationStrategy.Create(1),
                                    ColumnFamilies = columnFamilies,
                                },
                        },
                }, changeExistingKeyspaceMetadata : false);
            foreach (var columnFamily in columnFamilies)
                cassandraCluster.RetrieveColumnFamilyConnection(keyspaceName, columnFamily.Name).Truncate();
        }
    }
}