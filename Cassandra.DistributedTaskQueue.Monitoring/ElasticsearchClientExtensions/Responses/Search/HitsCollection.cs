using JetBrains.Annotations;

using Newtonsoft.Json;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions.Responses.Search
{
    internal class HitsCollection
    {
        [JsonProperty(PropertyName = "total")]
        [JsonConverter(typeof(TotalCountCompatibilityConverter))]
        public long TotalCount { get; set; }

        [NotNull, ItemNotNull]
        [JsonProperty(PropertyName = "hits")]
        public HitInfo[] Hits { get; set; }
    }
}