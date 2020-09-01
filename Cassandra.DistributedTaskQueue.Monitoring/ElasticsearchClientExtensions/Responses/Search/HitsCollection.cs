using JetBrains.Annotations;

using Newtonsoft.Json;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions.Responses.Search
{
    internal class HitsCollection
    {
        [JsonProperty(PropertyName = "total")]
        public int TotalCount { get; set; }

        [NotNull, ItemNotNull]
        [JsonProperty(PropertyName = "hits")]
        public HitInfo[] Hits { get; set; }
    }
}