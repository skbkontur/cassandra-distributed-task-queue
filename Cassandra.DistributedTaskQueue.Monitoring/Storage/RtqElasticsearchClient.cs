using Elasticsearch.Net;

using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage
{
    [PublicAPI]
    public class RtqElasticsearchClient : ElasticLowLevelClient, IRtqElasticsearchClient
    {
        public RtqElasticsearchClient()
        {
        }

        public RtqElasticsearchClient(IConnectionConfigurationValues settings)
            : base(settings)
        {
        }

        public RtqElasticsearchClient(string cloudId, BasicAuthenticationCredentials credentials)
            : base(cloudId, credentials)
        {
        }

        public RtqElasticsearchClient(ITransport<IConnectionConfigurationValues> transport)
            : base(transport)
        {
        }
    }
}