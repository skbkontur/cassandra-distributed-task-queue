using JetBrains.Annotations;

using Newtonsoft.Json;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api
{
    public class RtqMonitoringSearchRequest
    {
        [NotNull]
        [JsonProperty("enqueueTimestampRange")]
        public TimestampRange EnqueueTimestampRange { get; set; }

        [CanBeNull]
        [JsonProperty("queryString")]
        public string QueryString { get; set; }

        [CanBeNull]
        [JsonProperty("states")]
        public TaskState[] States { get; set; }

        [CanBeNull, ItemNotNull]
        [JsonProperty("names")]
        public string[] Names { get; set; }

        [JsonProperty("offset")]
        public int? Offset { get; set; }

        [JsonProperty("count")]
        public int? Count { get; set; }
    }
}