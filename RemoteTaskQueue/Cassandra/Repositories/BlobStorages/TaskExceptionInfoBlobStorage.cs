using GroBuf;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public class TaskExceptionInfoBlobStorage : BlobStorage<TaskExceptionInfo>, ITaskExceptionInfoBlobStorage
    {
        public TaskExceptionInfoBlobStorage(IColumnFamilyRepositoryParameters parameters, ISerializer serializer, IGlobalTime globalTime)
            : base(parameters, serializer, globalTime, columnFamilyName)
        {
        }

        public const string columnFamilyName = "taskExceptionInfo";
    }
}