using GroboContainer.Core;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Cassandra.CassandraClient.Scheme;

namespace RemoteTaskQueue.FunctionalTests
{
    public static class CassandraHelpers
    {
        public static void ResetCassandraState(this IContainer container, string keyspaceName, ColumnFamily[] columnFamilies)
        {
            var cassandraCluster = container.Get<ICassandraCluster>();
            cassandraCluster.ActualizeKeyspaces(new[]
                {
                    new KeyspaceScheme
                        {
                            Name = keyspaceName,
                            Configuration =
                                {
                                    ReplicationStrategy = SimpleReplicationStrategy.Create(1),
                                    ColumnFamilies = columnFamilies,
                                },
                        },
                });
            foreach(var columnFamily in columnFamilies)
                cassandraCluster.RetrieveColumnFamilyConnection(keyspaceName, columnFamily.Name).Truncate();
        }
    }
}