using Newtonsoft.Json;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions
{
    public class BulkResponse
    {
        [JsonProperty(PropertyName = "took")]
        public int TimeMs { get; set; }

        [JsonProperty(PropertyName = "errors")]
        public bool HasErrors { get; set; }

        [JsonProperty(PropertyName = "items")]
        public ItemInfo[] Items { get; set; }
    }

    public class ItemInfo
    {
        [JsonProperty(PropertyName = "index")]
        public IndexBulkResponse Index { get; set; }

        [JsonProperty(PropertyName = "create")]
        public ResponseBase Create { get; set; }

        [JsonProperty(PropertyName = "update")]
        public ResponseBase Update { get; set; }

        [JsonProperty(PropertyName = "delete")]
        public ResponseBase Delete { get; set; }
    }

    public class IndexBulkResponse : ResponseBase
    {
        [JsonProperty(PropertyName = "_index")]
        public string Index { get; set; }

        [JsonProperty(PropertyName = "_type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "_id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "_version")]
        public long Version { get; set; }
    }
}