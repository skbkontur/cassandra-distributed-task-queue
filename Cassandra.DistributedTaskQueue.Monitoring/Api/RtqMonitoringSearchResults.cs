using JetBrains.Annotations;

using Newtonsoft.Json;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api
{
    public class RtqMonitoringSearchResults
    {
        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [NotNull, ItemNotNull]
        [JsonProperty("taskMetas")]
        public RtqMonitoringTaskMeta[] TaskMetas { get; set; }
    }
}