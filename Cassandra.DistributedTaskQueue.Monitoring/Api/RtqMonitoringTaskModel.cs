using JetBrains.Annotations;

using Newtonsoft.Json;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api
{
    public class RtqMonitoringTaskModel
    {
        [NotNull]
        [JsonProperty("taskMeta")]
        public RtqMonitoringTaskMeta TaskMeta { get; set; }

        [NotNull]
        [JsonProperty("taskData")]
        [JsonConverter(typeof(TaskDataJsonSerializer))]
        public IRtqTaskData TaskData { get; set; }

        [NotNull, ItemNotNull]
        [JsonProperty("childTaskIds")]
        public string[] ChildTaskIds { get; set; }

        [NotNull, ItemNotNull]
        [JsonProperty("exceptionInfos")]
        public string[] ExceptionInfos { get; set; }
    }
}