namespace RemoteQueue.Cassandra.Primitives
{
    public interface IColumnFamilyCreator
    {
        void TryCreateColumnFamily(string columnFamilyName);
    }
}