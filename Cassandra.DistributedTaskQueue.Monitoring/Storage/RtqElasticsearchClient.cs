using System;
using System.Collections.Generic;

using Elasticsearch.Net;

using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage
{
    public class RtqElasticsearchClient : ElasticLowLevelClient, IRtqElasticsearchClient
    {
        public RtqElasticsearchClient([NotNull, ItemNotNull] IEnumerable<Uri> uris)
            : base(new ConnectionConfiguration(new SniffingConnectionPool(uris)).DisableDirectStreaming())
        {
        }

        public RtqElasticsearchClient(Uri uri)
            : base(new ConnectionConfiguration(new SingleNodeConnectionPool(uri)).DisableDirectStreaming())
        {
        }

        public bool UseElastic7 { get; set; }
    }
}