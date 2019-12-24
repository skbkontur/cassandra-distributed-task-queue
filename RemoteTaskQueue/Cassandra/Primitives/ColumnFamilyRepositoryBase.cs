using RemoteQueue.Settings;

using SkbKontur.Cassandra.ThriftClient.Clusters;
using SkbKontur.Cassandra.ThriftClient.Connections;

namespace RemoteQueue.Cassandra.Primitives
{
    public abstract class ColumnFamilyRepositoryBase
    {
        protected ColumnFamilyRepositoryBase(ICassandraCluster cassandraCluster, IRemoteTaskQueueSettings settings, string columnFamilyName)
        {
            this.cassandraCluster = cassandraCluster;
            keyspace = settings.QueueKeyspace;
            this.columnFamilyName = columnFamilyName;
        }

        protected IColumnFamilyConnection RetrieveColumnFamilyConnection()
        {
            return cassandraCluster.RetrieveColumnFamilyConnection(keyspace, columnFamilyName);
        }

        private readonly ICassandraCluster cassandraCluster;
        private readonly string keyspace;
        private readonly string columnFamilyName;
    }
}