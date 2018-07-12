using JetBrains.Annotations;

using SKBKontur.Cassandra.CassandraClient.Abstractions;

namespace RemoteQueue.Configuration
{
    public interface IColumnFamilyRegistry
    {
        [NotNull]
        ColumnFamily[] GetAllColumnFamilyNames();
    }
}