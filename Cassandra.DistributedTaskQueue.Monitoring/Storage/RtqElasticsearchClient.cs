using System;
using System.Collections.Generic;

using Elasticsearch.Net;

using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage
{
    public class RtqElasticsearchClient : ElasticLowLevelClient, IRtqElasticsearchClient
    {
        public RtqElasticsearchClient([NotNull, ItemNotNull] IEnumerable<Uri> uris)
            : this(new SniffingConnectionPool(uris))
        {
        }

        public RtqElasticsearchClient(IConnectionPool connectionPool)
            : base(new ConnectionConfiguration(connectionPool).DisableDirectStreaming())
        {
        }

        public bool UseElastic7 { get; set; }
    }
}