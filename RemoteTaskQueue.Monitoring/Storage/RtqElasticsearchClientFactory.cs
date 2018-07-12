using SKBKontur.Catalogue.Core.Configuration.Settings.TopologySearch;
using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;

namespace RemoteTaskQueue.Monitoring.Storage
{
    public class RtqElasticsearchClientFactory : ElasticsearchClientFactory
    {
        public RtqElasticsearchClientFactory(TopologyDependencies topologyDependencies)
            : base("elasticsearchRtq", topologyDependencies)
        {
        }
    }
}