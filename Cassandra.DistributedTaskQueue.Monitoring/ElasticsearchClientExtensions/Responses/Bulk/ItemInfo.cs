using System.Text.Json.Serialization;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions.Responses.Bulk;

internal class ItemInfo
{
    [JsonPropertyName("index")]
    public IndexBulkResponse Index { get; set; }

    [JsonPropertyName("create")]
    public ResponseBase Create { get; set; }

    [JsonPropertyName("update")]
    public ResponseBase Update { get; set; }

    [JsonPropertyName("delete")]
    public ResponseBase Delete { get; set; }
}