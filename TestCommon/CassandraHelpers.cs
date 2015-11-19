using GroboContainer.Core;

using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Cassandra.CassandraClient.Scheme;

namespace TestCommon
{
    public static class CassandraHelpers
    {
        public static void DropAndCreateDatabase(this IContainer container, ColumnFamily[] columnFamilies)
        {
            var settings = container.Get<ICassandraSettings>();
            var cassandraCluster = container.Get<ICassandraCluster>();
            cassandraCluster.ActualizeKeyspaces(new[]
                {
                    new KeyspaceScheme
                        {
                            Name = settings.QueueKeyspace,
                            Configuration =
                                {
                                    ReplicationFactor = 1,
                                    ReplicaPlacementStrategy = ReplicaPlacementStrategy.Simple,
                                    ColumnFamilies = columnFamilies,
                                }
                        },
                });
            foreach(var columnFamily in columnFamilies)
                cassandraCluster.RetrieveColumnFamilyConnection(settings.QueueKeyspace, columnFamily.Name).Truncate();
        }
    }
}