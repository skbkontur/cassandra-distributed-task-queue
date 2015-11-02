using GroBuf;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public class TaskExceptionInfoBlobStorage : BlobStorageDecorator<TaskExceptionInfo>, ITaskExceptionInfoBlobStorage
    {
        public TaskExceptionInfoBlobStorage(IColumnFamilyRepositoryParameters parameters, ISerializer serializer, IGlobalTime globalTime)
            : base(parameters, serializer, globalTime, columnFamilyName, orderedColumnFamilyName)
        {
        }

        public const string columnFamilyName = "taskExceptionInfo";
        public const string orderedColumnFamilyName = "orderedTaskExceptionInfo";
    }
}