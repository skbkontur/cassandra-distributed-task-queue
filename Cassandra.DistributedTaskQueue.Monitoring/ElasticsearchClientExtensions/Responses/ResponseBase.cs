using System.Text.Json.Serialization;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions.Responses;

internal class ResponseBase
{
    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("error")]
    public object Error { get; set; }
}