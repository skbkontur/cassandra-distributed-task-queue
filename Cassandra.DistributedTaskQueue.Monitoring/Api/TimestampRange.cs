using JetBrains.Annotations;

using Newtonsoft.Json;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api
{
    public class TimestampRange
    {
        [NotNull]
        [JsonProperty("lowerBound")]
        [JsonConverter(typeof(TimestampJsonConverter))]
        public Timestamp LowerBound { get; set; }

        [NotNull]
        [JsonProperty("upperBound")]
        [JsonConverter(typeof(TimestampJsonConverter))]
        public Timestamp UpperBound { get; set; }
    }
}