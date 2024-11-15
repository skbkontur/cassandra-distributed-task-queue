#nullable disable

using System.Text.Json.Serialization;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api;

public class TimestampRange
{
    [JsonPropertyName("lowerBound")]
    [JsonConverter(typeof(TimestampJsonConverter))]
    public Timestamp LowerBound { get; set; }

    [JsonPropertyName("upperBound")]
    [JsonConverter(typeof(TimestampJsonConverter))]
    public Timestamp UpperBound { get; set; }
}