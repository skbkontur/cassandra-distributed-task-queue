using System.Text.Json.Serialization;

using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api;

public class RtqMonitoringSearchResults
{
    [JsonPropertyName("totalCount")]
    public long TotalCount { get; set; }

    [NotNull, ItemNotNull]
    [JsonPropertyName("taskMetas")]
    public RtqMonitoringTaskMeta[] TaskMetas { get; set; } = null!;
}