using JetBrains.Annotations;

using SkbKontur.Cassandra.ThriftClient.Abstractions;

namespace RemoteQueue.Configuration
{
    public interface IColumnFamilyRegistry
    {
        [NotNull]
        ColumnFamily[] GetAllColumnFamilyNames();
    }
}