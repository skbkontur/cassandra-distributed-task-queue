using Newtonsoft.Json;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions.Responses.Bulk
{
    internal class BulkResponse
    {
        [JsonProperty(PropertyName = "took")]
        public int TimeMs { get; set; }

        [JsonProperty(PropertyName = "errors")]
        public bool HasErrors { get; set; }

        [JsonProperty(PropertyName = "items")]
        public ItemInfo[] Items { get; set; }
    }
}