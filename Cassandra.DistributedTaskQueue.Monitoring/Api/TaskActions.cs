using System.Text.Json.Serialization;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api;

public class TaskActions
{
    [JsonPropertyName("canCancel")]
    public bool CanCancel { get; set; }

    [JsonPropertyName("canRerun")]
    public bool CanRerun { get; set; }
}