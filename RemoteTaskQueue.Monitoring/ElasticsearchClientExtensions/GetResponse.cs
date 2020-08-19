using Newtonsoft.Json;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions
{
    public class GetResponse<T>
    {
        [JsonProperty(PropertyName = "_index")]
        public string Index { get; set; }

        [JsonProperty(PropertyName = "_type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "_id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "_version")]
        public long Version { get; set; }

        [JsonProperty(PropertyName = "found")]
        public bool Found { get; set; }

        [JsonProperty(PropertyName = "_source")]
        public T Source { get; set; }
    }
}