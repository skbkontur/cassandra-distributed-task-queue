using System.Collections.Generic;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public interface IBlobStorage<T, TId>
    {
        void Write(TId id, T element);
        bool TryWrite(T element, out TId id);
        T Read(TId id);
        Dictionary<TId, T> Read(TId[] ids);
        IEnumerable<KeyValuePair<TId, T>> ReadAll(int batchSize = 1000);
        void Delete(TId id, long timestamp);
        void Delete(IEnumerable<TId> ids, long? timestamp);
    }

    public interface IBlobStorage<T> : IBlobStorage<T, string>
    {
    }
}