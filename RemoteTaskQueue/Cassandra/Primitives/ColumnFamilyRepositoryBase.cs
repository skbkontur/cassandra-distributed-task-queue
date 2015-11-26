using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Cassandra.CassandraClient.Connections;

namespace RemoteQueue.Cassandra.Primitives
{
    public abstract class ColumnFamilyRepositoryBase : IColumnFamilyRepository
    {
        protected ColumnFamilyRepositoryBase(IColumnFamilyRepositoryParameters parameters, string columnFamilyName)
        {
            cassandraCluster = parameters.CassandraCluster;
            ColumnFamilyName = columnFamilyName;
            Keyspace = parameters.Settings.QueueKeyspace;
        }

        public IColumnFamilyConnection RetrieveColumnFamilyConnection()
        {
            return cassandraCluster.RetrieveColumnFamilyConnection(Keyspace, ColumnFamilyName);
        }

        public string Keyspace { get; private set; }
        public string ColumnFamilyName { get; private set; }
        private readonly ICassandraCluster cassandraCluster;
    }
}