using System.Collections.Generic;

namespace RemoteQueue.Cassandra.Primitives
{
    public interface IBlobStorage<T>
    {
        void Write(string id, T element);
        T Read(string id);
        T[] Read(string[] ids);
        T[] ReadQuiet(string[] taskIds); 
        IEnumerable<T> ReadAll(int batchSize = 1000);

        void Delete(string id, long timestamp);
        void Delete(string[] ids, long? timestamp);
    }
}