using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Clusters;

namespace RemoteQueue.Cassandra.Primitives
{
    public class ColumnFamilyRepositoryParameters : IColumnFamilyRepositoryParameters
    {
        public ColumnFamilyRepositoryParameters(ICassandraCluster cassandraCluster, ICassandraSettings settings)
        {
            CassandraCluster = cassandraCluster;
            Settings = settings;
        }

        public ICassandraCluster CassandraCluster { get; private set; }
        public ICassandraSettings Settings { get; private set; }
    }
}