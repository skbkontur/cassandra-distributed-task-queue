using System.Text.Json.Serialization;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions.Responses.Bulk;

internal class BulkResponse
{
    [JsonPropertyName("took")]
    public int TimeMs { get; set; }

    [JsonPropertyName("errors")]
    public bool HasErrors { get; set; }

    [JsonPropertyName("items")]
    public ItemInfo[] Items { get; set; }
}