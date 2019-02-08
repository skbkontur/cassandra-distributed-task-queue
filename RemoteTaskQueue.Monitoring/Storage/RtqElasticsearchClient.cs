using System;

using Elasticsearch.Net;

using JetBrains.Annotations;

namespace RemoteTaskQueue.Monitoring.Storage
{
    public class RtqElasticsearchClient : ElasticLowLevelClient, IRtqElasticsearchClient
    {
        public RtqElasticsearchClient([NotNull, ItemNotNull] params Uri[] uris)
            : base(new ConnectionConfiguration(new SniffingConnectionPool(uris)).DisableDirectStreaming())
        {
        }
    }
}