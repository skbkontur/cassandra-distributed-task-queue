using System.Linq;

using GroboContainer.Core;

using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;

namespace TestCommon
{
    public class CassandraHelpers
    {
        public static void DropAndCreateDatabase(ColumnFamily[] columnFamilies, IContainer container)
        {
            var settings = container.Get<ICassandraSettings>();
            var cassandraCluster = container.Get<ICassandraCluster>();
            var clusterConnection = cassandraCluster.RetrieveClusterConnection();
            var keyspaceConnection = cassandraCluster.RetrieveKeyspaceConnection(settings.QueueKeyspace);

            var keyspaces = clusterConnection.RetrieveKeyspaces();
            if (keyspaces.All(x => x.Name != settings.QueueKeyspace))
            {
                clusterConnection.AddKeyspace(
                    new Keyspace
                    {
                        Name = settings.QueueKeyspace,
                        ReplicaPlacementStrategy = "org.apache.cassandra.locator.SimpleStrategy",
                        ReplicationFactor = 1
                    });
            }

            var cassandraColumnFamilies = keyspaceConnection.DescribeKeyspace().ColumnFamilies;
            foreach (var columnFamily in columnFamilies)
            {
                if (!cassandraColumnFamilies.Any(x => x.Key == columnFamily.Name))
                    keyspaceConnection.AddColumnFamily(columnFamily);
            }

            foreach (var columnFamily in columnFamilies)
            {
                var columnFamilyConnection = cassandraCluster.RetrieveColumnFamilyConnection(settings.QueueKeyspace, columnFamily.Name);
                columnFamilyConnection.Truncate();
            }
        }
 
    }
}