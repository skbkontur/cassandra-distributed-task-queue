namespace RemoteQueue.Cassandra.Primitives
{
    public interface IBlobStorage<T>
    {
        void Write(string id, T element);
        T Read(string id);
    }
}