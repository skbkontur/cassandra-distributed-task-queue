using System.Text.Json.Serialization;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api;

public class TimestampRange
{
    [NotNull]
    [JsonPropertyName("lowerBound")]
    [JsonConverter(typeof(TimestampJsonConverter))]
    public Timestamp LowerBound { get; set; } = null!;

    [NotNull]
    [JsonPropertyName("upperBound")]
    [JsonConverter(typeof(TimestampJsonConverter))]
    public Timestamp UpperBound { get; set; } = null!;
}