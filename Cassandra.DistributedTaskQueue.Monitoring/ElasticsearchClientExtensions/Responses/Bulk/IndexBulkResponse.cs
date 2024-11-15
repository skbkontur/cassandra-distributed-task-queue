using System.Text.Json.Serialization;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions.Responses.Bulk;

internal class IndexBulkResponse : ResponseBase
{
    [JsonPropertyName("_index")]
    public string Index { get; set; }

    [JsonPropertyName("_type")]
    public string Type { get; set; }

    [JsonPropertyName("_id")]
    public string Id { get; set; }

    [JsonPropertyName("_version")]
    public long Version { get; set; }
}