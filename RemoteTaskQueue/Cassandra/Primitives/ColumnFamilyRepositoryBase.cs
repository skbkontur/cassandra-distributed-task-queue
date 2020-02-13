using SkbKontur.Cassandra.DistributedTaskQueue.Settings;
using SkbKontur.Cassandra.ThriftClient.Clusters;
using SkbKontur.Cassandra.ThriftClient.Connections;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Primitives
{
    public abstract class ColumnFamilyRepositoryBase
    {
        protected ColumnFamilyRepositoryBase(ICassandraCluster cassandraCluster, IRtqSettings rtqSettings, string columnFamilyName)
        {
            this.cassandraCluster = cassandraCluster;
            keyspace = rtqSettings.QueueKeyspace;
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