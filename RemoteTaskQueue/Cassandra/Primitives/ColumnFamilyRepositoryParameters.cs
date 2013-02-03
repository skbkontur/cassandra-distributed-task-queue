using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Clusters;

namespace RemoteQueue.Cassandra.Primitives
{
    public class ColumnFamilyRepositoryParameters : IColumnFamilyRepositoryParameters
    {
        public ColumnFamilyRepositoryParameters(
        ICassandraCluster cassandraCluster, 
        ICassandraClusterSettings settings,
        IRemoteTaskQueueCassandraSettings remoteTaskQueueCassandraSettings)
        {
            CassandraCluster = cassandraCluster;
            Settings = settings;
            RemoteTaskQueueCassandraSettings = remoteTaskQueueCassandraSettings;
        }

        public ICassandraCluster CassandraCluster { get; private set; }
        public ICassandraClusterSettings Settings { get; private set; }
        public IRemoteTaskQueueCassandraSettings RemoteTaskQueueCassandraSettings { get; private set; }

    }
}