using System.Collections.Generic;

namespace RemoteQueue.Cassandra.Primitives
{
    public interface IBlobStorage<T, TId>
    {
        BlobWriteResult Write(TId id, T element);
        T Read(TId id);
        Dictionary<TId, T> Read(IEnumerable<TId> ids);
        IEnumerable<T> ReadAll(int batchSize = 1000);
        IEnumerable<KeyValuePair<TId, T>> ReadAllWithIds(int batchSize = 1000);
        void Delete(TId id, long timestamp);
        void Delete(IEnumerable<TId> ids, long? timestamp);
    }

    public interface IBlobStorage<T> : IBlobStorage<T, string>
    {
    }
}