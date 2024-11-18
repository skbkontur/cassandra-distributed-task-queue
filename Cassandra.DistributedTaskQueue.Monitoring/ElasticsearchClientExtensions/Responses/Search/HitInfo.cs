#nullable enable

using System.Text.Json.Serialization;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions.Responses.Search;

internal class HitInfo
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } = null!;
}