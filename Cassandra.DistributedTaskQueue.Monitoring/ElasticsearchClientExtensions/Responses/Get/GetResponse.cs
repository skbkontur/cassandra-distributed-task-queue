using System.Text.Json.Serialization;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions.Responses.Get;

internal class GetResponse<T>
{
    [JsonPropertyName("_index")]
    public string Index { get; set; }

    [JsonPropertyName("_type")]
    public string Type { get; set; }

    [JsonPropertyName("_id")]
    public string Id { get; set; }

    [JsonPropertyName("_version")]
    public long Version { get; set; }

    [JsonPropertyName("found")]
    public bool Found { get; set; }

    [JsonPropertyName("_source")]
    public T Source { get; set; }
}