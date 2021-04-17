using GroboContainer.Core;

using RemoteTaskQueue.FunctionalTests.Common;

using SkbKontur.Cassandra.ThriftClient.Abstractions;
using SkbKontur.Cassandra.ThriftClient.Clusters;
using SkbKontur.Cassandra.ThriftClient.Schema;

using Vostok.Logging.Abstractions;

namespace RemoteTaskQueue.FunctionalTests
{
    public static class CassandraHelpers
    {
        public static void ResetCassandraState(this IContainer container, ColumnFamily[] columnFamilies)
        {
            var logger = container.Get<ILog>();
            var cassandraCluster = container.Get<ICassandraCluster>();
            var schemaActualizer = new CassandraSchemaActualizer(cassandraCluster, eventListener : null, logger);
            schemaActualizer.ActualizeKeyspaces(new[]
                                                    {
                                                        new KeyspaceSchema
                                                            {
                                                                Name = TestRtqSettings.QueueKeyspaceName,
                                                                Configuration =
                                                                    {
                                                                        ReplicationStrategy = SimpleReplicationStrategy.Create(1),
                                                                        ColumnFamilies = columnFamilies,
                                                                    },
                                                            },
                                                    },
                                                changeExistingKeyspaceMetadata : false);
            foreach (var columnFamily in columnFamilies)
                cassandraCluster.RetrieveColumnFamilyConnection(TestRtqSettings.QueueKeyspaceName, columnFamily.Name).Truncate();
        }
    }
}