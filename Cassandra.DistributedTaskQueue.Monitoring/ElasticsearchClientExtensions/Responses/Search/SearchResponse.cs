using System.Text.Json.Serialization;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions.Responses.Search;

internal class SearchResponse
{
    [JsonPropertyName("hits")]
    public HitsCollection Hits { get; set; }
}