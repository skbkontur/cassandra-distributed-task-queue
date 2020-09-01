using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api
{
    public class RtqMonitoringTaskModel
    {
        [NotNull]
        public TaskMetaInformation TaskMeta { get; set; }

        [NotNull]
        public IRtqTaskData TaskData { get; set; }

        [NotNull, ItemNotNull]
        public string[] ChildTaskIds { get; set; }

        [NotNull, ItemNotNull]
        public TaskExceptionInfo[] ExceptionInfos { get; set; }
    }
}