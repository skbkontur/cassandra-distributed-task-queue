using System.Text.Json.Serialization;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api;

public class RtqMonitoringTaskMeta
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("ticks")]
    [JsonConverter(typeof(LongToStringConverter))]
    public long Ticks { get; set; }

    [JsonPropertyName("minimalStartTicks")]
    [JsonConverter(typeof(LongToStringConverter))]
    public long MinimalStartTicks { get; set; }

    [JsonPropertyName("startExecutingTicks")]
    [JsonConverter(typeof(LongToStringConverter))]
    public long? StartExecutingTicks { get; set; }

    [JsonPropertyName("finishExecutingTicks")]
    [JsonConverter(typeof(LongToStringConverter))]
    public long? FinishExecutingTicks { get; set; }

    [JsonPropertyName("lastModificationTicks")]
    [JsonConverter(typeof(LongToStringConverter))]
    public long? LastModificationTicks { get; set; }

    [JsonPropertyName("expirationTimestampTicks")]
    [JsonConverter(typeof(LongToStringConverter))]
    public long? ExpirationTimestampTicks { get; set; }

    [JsonPropertyName("expirationModificationTicks")]
    [JsonConverter(typeof(LongToStringConverter))]
    public long? ExpirationModificationTicks { get; set; }

    [JsonPropertyName("executionDurationTicks")]
    [JsonConverter(typeof(LongToStringConverter))]
    public long? ExecutionDurationTicks { get; set; }

    [JsonPropertyName("state")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TaskState State { get; set; }

    [JsonPropertyName("taskActions")]
    public TaskActions TaskActions { get; set; }

    [JsonPropertyName("attempts")]
    public int Attempts { get; set; }

    [JsonPropertyName("parentTaskId")]
    public string ParentTaskId { get; set; }
}