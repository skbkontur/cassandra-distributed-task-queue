#nullable disable

using System.Text.Json.Serialization;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api;

public class RtqMonitoringTaskModel
{
    public static RtqMonitoringTaskModel Empty => new();

    [JsonPropertyName("taskMeta")]
    public RtqMonitoringTaskMeta TaskMeta { get; set; }

    [JsonPropertyName("taskData")]
    [JsonConverter(typeof(TaskDataJsonSerializer))]
    public IRtqTaskData TaskData { get; set; }

    [JsonPropertyName("childTaskIds")]
    public string[] ChildTaskIds { get; set; }

    [JsonPropertyName("exceptionInfos")]
    public string[] ExceptionInfos { get; set; }
}