using RemoteQueue.Cassandra.Primitives;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public interface ITaskDataBlobStorage : IBlobStorage<byte[]>
    {
    }
}