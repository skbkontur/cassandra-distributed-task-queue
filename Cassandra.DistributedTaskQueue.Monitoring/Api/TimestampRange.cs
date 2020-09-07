using JetBrains.Annotations;

using Newtonsoft.Json;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api
{
    public class TimestampRange
    {
        [NotNull]
        [JsonConverter(typeof(TimestampJsonConverter))]
        public Timestamp LowerBound { get; set; }

        [NotNull]
        [JsonConverter(typeof(TimestampJsonConverter))]
        public Timestamp UpperBound { get; set; }
    }
}