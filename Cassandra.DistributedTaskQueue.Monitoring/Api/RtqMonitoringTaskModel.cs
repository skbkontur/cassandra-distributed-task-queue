#nullable enable

using System.Text.Json.Serialization;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api;

public class RtqMonitoringTaskModel
{
    public static RtqMonitoringTaskModel Empty => new();

    [JsonPropertyName("taskMeta")]
    public RtqMonitoringTaskMeta TaskMeta { get; set; } = null!;

    [JsonPropertyName("taskData")]
    [JsonConverter(typeof(TaskDataJsonSerializer))]
    public IRtqTaskData TaskData { get; set; } = null!;

    [JsonPropertyName("childTaskIds")]
    public string[] ChildTaskIds { get; set; } = null!;

    [JsonPropertyName("exceptionInfos")]
    public string[] ExceptionInfos { get; set; } = null!;
}