using SKBKontur.Cassandra.CassandraClient.Clusters;

namespace RemoteQueue.Settings
{
    public interface ICassandraSettings : ICassandraClusterSettings
    {
        string QueueKeyspace { get; }
    }
}