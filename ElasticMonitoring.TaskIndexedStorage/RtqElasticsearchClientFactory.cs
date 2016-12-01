using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage
{
    public class RtqElasticsearchClientFactory : ElasticsearchClientFactory
    {
        public RtqElasticsearchClientFactory(IElasticsearchClientCreator clientCreator)
            : base("elasticsearchRtq", clientCreator)
        {
        }
    }
}