using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Primitives;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public interface ITaskMetaInformationBlobStorage : IBlobStorage<TaskMetaInformation>
    {
        
    }
}