using JetBrains.Annotations;

using Newtonsoft.Json;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions
{
    public class SearchResponse
    {
        [NotNull]
        [JsonProperty(PropertyName = "hits")]
        public HitsCollection Hits { get; set; }
    }

    public class HitsCollection
    {
        [JsonProperty(PropertyName = "total")]
        public int TotalCount { get; set; }

        [NotNull, ItemNotNull]
        [JsonProperty(PropertyName = "hits")]
        public HitInfo[] Hits { get; set; }
    }

    public class HitInfo
    {
        [NotNull]
        [JsonProperty(PropertyName = "_id")]
        public string Id { get; set; }
    }
}