using JetBrains.Annotations;

using System.Text.Json.Serialization;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api;

public class RtqMonitoringTaskModel
{
    public static RtqMonitoringTaskModel Empty => new RtqMonitoringTaskModel();

    [NotNull]
    [JsonPropertyName("taskMeta")]
    public RtqMonitoringTaskMeta TaskMeta { get; set; }

    [NotNull]
    [JsonPropertyName("taskData")]
    [JsonConverter(typeof(TaskDataJsonSerializer))]
    public IRtqTaskData TaskData { get; set; }

    [NotNull, ItemNotNull]
    [JsonPropertyName("childTaskIds")]
    public string[] ChildTaskIds { get; set; }

    [NotNull, ItemNotNull]
    [JsonPropertyName("exceptionInfos")]
    public string[] ExceptionInfos { get; set; }
}