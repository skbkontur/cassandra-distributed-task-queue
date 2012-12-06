using GroBuf;

using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public class TaskDataBlobStorage : BlobStorage<byte[]>, ITaskDataBlobStorage
    {
        public TaskDataBlobStorage(IColumnFamilyRepositoryParameters parameters, ISerializer serializer, IGlobalTime globalTime)
            : base(parameters, serializer, globalTime, columnFamilyName)
        {
        }

        public const string columnFamilyName = "taskDataStorage";
    }
}