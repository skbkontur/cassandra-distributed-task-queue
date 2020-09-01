using Newtonsoft.Json;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions.Responses.Bulk
{
    internal class ItemInfo
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
}