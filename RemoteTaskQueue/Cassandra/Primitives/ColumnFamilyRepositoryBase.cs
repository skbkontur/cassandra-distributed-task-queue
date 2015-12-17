using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Cassandra.CassandraClient.Connections;

namespace RemoteQueue.Cassandra.Primitives
{
    public abstract class ColumnFamilyRepositoryBase
    {
        protected ColumnFamilyRepositoryBase(IColumnFamilyRepositoryParameters parameters, string columnFamilyName)
        {
            cassandraCluster = parameters.CassandraCluster;
            keyspace = parameters.Settings.QueueKeyspace;
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