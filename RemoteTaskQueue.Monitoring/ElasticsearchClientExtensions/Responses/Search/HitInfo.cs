using JetBrains.Annotations;

using Newtonsoft.Json;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions.Responses.Search
{
    internal class HitInfo
    {
        [NotNull]
        [JsonProperty(PropertyName = "_id")]
        public string Id { get; set; }
    }
}