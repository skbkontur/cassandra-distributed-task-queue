using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Clusters;

namespace RemoteQueue.Cassandra.Primitives
{
    public interface IColumnFamilyRepositoryParameters
    {
        ICassandraCluster CassandraCluster { get; }
        ICassandraSettings Settings { get; }
    }
}