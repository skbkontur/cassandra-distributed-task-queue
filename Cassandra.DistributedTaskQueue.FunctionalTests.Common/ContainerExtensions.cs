using System;

using Elasticsearch.Net;

using GroboContainer.Core;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage;

namespace SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common
{
    public static class ContainerExtensions
    {
        public static void ConfigureRtqElasticClient(this IContainer container)
        {
            var elasticUrl = Environment.GetEnvironmentVariable("ES_URL") ?? "http://localhost:9205";
            var connectionPool = new SingleNodeConnectionPool(new Uri(elasticUrl));
            var configuration = new ConnectionConfiguration(connectionPool).DisableDirectStreaming();
            container.Configurator.ForAbstraction<IRtqElasticsearchClient>().UseInstances(new RtqElasticsearchClient(configuration));
        }
    }
}