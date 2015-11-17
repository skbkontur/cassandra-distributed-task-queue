using GroBuf;

using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public class TaskDataBlobStorage : IntermideateBlobStorageDecorator<byte[]>, ITaskDataBlobStorage
    {
        public TaskDataBlobStorage(IColumnFamilyRepositoryParameters parameters, ISerializer serializer, IGlobalTime globalTime)
            : base(parameters, serializer, globalTime, columnFamilyName, timeBasedColumnFamilyName)
        {
        }

        public const string columnFamilyName = "taskDataStorage";
        public const string timeBasedColumnFamilyName = "timeBasedTaskDataStorage";
    }
}