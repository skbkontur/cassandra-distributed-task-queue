using SKBKontur.Cassandra.CassandraClient.Connections;

namespace RemoteQueue.Cassandra.Primitives
{
    public interface IColumnFamilyRepository
    {
        IColumnFamilyConnection RetrieveColumnFamilyConnection();
        string ColumnFamilyName { get; }
    }
}