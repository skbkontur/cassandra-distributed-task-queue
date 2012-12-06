using SKBKontur.Cassandra.CassandraClient.Abstractions;

namespace RemoteQueue.Configuration
{
    public interface IColumnFamilyRegistry
    {
        ColumnFamily[] GetAllColumnFamilyNames();
    }
}