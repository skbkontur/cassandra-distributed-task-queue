#nullable disable

using System.Text.Json.Serialization;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions.Responses.Search;

internal class HitsCollection
{
    [JsonPropertyName("total")]
    [JsonConverter(typeof(TotalCountCompatibilityConverter))]
    public long TotalCount { get; set; }

    [JsonPropertyName("hits")]
    public HitInfo[] Hits { get; set; }
}