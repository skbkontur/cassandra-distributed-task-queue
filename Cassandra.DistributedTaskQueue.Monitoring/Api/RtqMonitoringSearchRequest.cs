using System.Text.Json.Serialization;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api;

public class RtqMonitoringSearchRequest
{
    [JsonPropertyName("enqueueTimestampRange")]
    public TimestampRange EnqueueTimestampRange { get; set; }

#nullable enable
    [JsonPropertyName("queryString")]
    public string? QueryString { get; set; }

    [JsonPropertyName("states")]
    public TaskState[]? States { get; set; }

#nullable disable
    [JsonPropertyName("names")]
    public string[] Names { get; set; }

    [JsonPropertyName("offset")]
    public int? Offset { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }
}