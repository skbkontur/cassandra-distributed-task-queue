using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.Core.InternalApi.Core;

namespace RemoteTaskQueue.Monitoring.Api
{
    public class TaskMetaInformationChildTasks
    {
        public string[] ChildTaskIds { get; set; }
    }

    [InternalAPI]
    public class RemoteTaskInfoModel
    {
        [NotNull]
        public Merged<TaskMetaInformation, TaskMetaInformationChildTasks> TaskMeta { get; set; }

        [NotNull]
        public ITaskData TaskData { get; set; }

        [NotNull]
        public TaskExceptionInfo[] ExceptionInfos { get; set; }
    }
}