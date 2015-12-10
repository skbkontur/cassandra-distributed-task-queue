using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public interface ITaskExceptionInfoBlobStorage : IBlobStorage<TaskExceptionInfo>
    {
    }
}