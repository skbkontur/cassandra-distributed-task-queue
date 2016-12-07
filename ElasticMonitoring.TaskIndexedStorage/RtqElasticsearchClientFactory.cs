using SKBKontur.Catalogue.Core.Configuration.Settings.TopologySearch;
using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage
{
    public class RtqElasticsearchClientFactory : ElasticsearchClientFactory
    {
        public RtqElasticsearchClientFactory(TopologyDependencies topologyDependencies)
            : base("elasticsearchRtq", topologyDependencies)
        {
        }
    }
}