using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Cassandra.CassandraClient.Connections;

namespace RemoteQueue.Cassandra.Primitives
{
    public abstract class ColumnFamilyRepositoryBase : IColumnFamilyRepository
    {
        protected ColumnFamilyRepositoryBase(IColumnFamilyRepositoryParameters parameters, string columnFamilyName)
        {
            cassandraCluster = parameters.CassandraCluster;
            remoteTaskQueueCassandraSettings = parameters.RemoteTaskQueueCassandraSettings;
            ColumnFamilyName = columnFamilyName;
        }

        public IColumnFamilyConnection RetrieveColumnFamilyConnection()
        {
            return cassandraCluster.RetrieveColumnFamilyConnection(remoteTaskQueueCassandraSettings.QueueKeyspace, ColumnFamilyName);
        }

        public string ColumnFamilyName { get; private set; }
        private readonly ICassandraCluster cassandraCluster;
        private readonly IRemoteTaskQueueCassandraSettings remoteTaskQueueCassandraSettings;
    }
}