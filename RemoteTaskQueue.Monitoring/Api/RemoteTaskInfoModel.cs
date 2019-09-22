using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

namespace RemoteTaskQueue.Monitoring.Api
{
    public class RemoteTaskInfoModel
    {
        [NotNull]
        public TaskMetaInformation TaskMeta { get; set; }

        [NotNull]
        public ITaskData TaskData { get; set; }

        [NotNull, ItemNotNull]
        public string[] ChildTaskIds { get; set; }

        [NotNull, ItemNotNull]
        public TaskExceptionInfo[] ExceptionInfos { get; set; }
    }
}