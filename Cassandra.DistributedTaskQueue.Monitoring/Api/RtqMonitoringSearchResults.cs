using System.Text.Json.Serialization;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api;

public class RtqMonitoringSearchResults
{
    [JsonPropertyName("totalCount")]
    public long TotalCount { get; set; }

#nullable disable
    [JsonPropertyName("taskMetas")]
    public RtqMonitoringTaskMeta[] TaskMetas { get; set; }
}