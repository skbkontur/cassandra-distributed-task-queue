using Elasticsearch.Net;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage
{
    public interface IRtqElasticsearchClient : IElasticLowLevelClient
    {
        bool UseElastic7 { get; }
    }
}