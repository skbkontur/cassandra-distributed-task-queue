using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public interface ITaskMetaInformationBlobStorage : IBlobStorage<TaskMetaInformation>
    {
    }
}