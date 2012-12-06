using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Primitives;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public interface ITaskExceptionInfoBlobStorage : IBlobStorage<TaskExceptionInfo>
    {
    }
}