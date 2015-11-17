using GroBuf;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public class TaskMetaInformationBlobStorage : IntermideateBlobStorageDecorator<TaskMetaInformation>, ITaskMetaInformationBlobStorage
    {
        public TaskMetaInformationBlobStorage(IColumnFamilyRepositoryParameters parameters, ISerializer serializer, IGlobalTime globalTime)
            : base(parameters, serializer, globalTime, columnFamilyName, timeBasedColumnFamilyName)
        {
        }

        public const string columnFamilyName = "taskMetaInformation";
        public const string timeBasedColumnFamilyName = "timeBasedTaskMetaInformation";
    }
}