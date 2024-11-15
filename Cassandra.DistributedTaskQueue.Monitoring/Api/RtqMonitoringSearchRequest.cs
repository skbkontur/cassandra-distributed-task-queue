using System.Text.Json.Serialization;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api;

public class RtqMonitoringSearchRequest
{
    [NotNull]
    [JsonPropertyName("enqueueTimestampRange")]
    public TimestampRange EnqueueTimestampRange { get; set; }

    [CanBeNull]
    [JsonPropertyName("queryString")]
    public string QueryString { get; set; }

    [CanBeNull]
    [JsonPropertyName("states")]
    public TaskState[] States { get; set; }

    [CanBeNull, ItemNotNull]
    [JsonPropertyName("names")]
    public string[] Names { get; set; }

    [JsonPropertyName("offset")]
    public int? Offset { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }
}