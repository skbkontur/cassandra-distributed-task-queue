using JetBrains.Annotations;

using Newtonsoft.Json;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions.Responses.Search
{
    internal class SearchResponse
    {
        [NotNull]
        [JsonProperty(PropertyName = "hits")]
        public HitsCollection Hits { get; set; }
    }
}