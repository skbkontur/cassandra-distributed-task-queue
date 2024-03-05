using Newtonsoft.Json;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api;

public class TaskActions
{
    [JsonProperty("canCancel")]
    public bool CanCancel { get; set; }

    [JsonProperty("canRerun")]
    public bool CanRerun { get; set; }
}