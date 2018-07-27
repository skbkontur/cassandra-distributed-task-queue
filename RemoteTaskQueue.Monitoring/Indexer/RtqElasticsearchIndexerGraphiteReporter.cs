using SKBKontur.Catalogue.Core.Graphite.Client.Settings;
using SKBKontur.Catalogue.Core.Graphite.Client.StatsD;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class RtqElasticsearchIndexerGraphiteReporter : RtqElasticsearchIndexerGraphiteReporterBase
    {
        public RtqElasticsearchIndexerGraphiteReporter(ICatalogueStatsDClient statsDClient, IGraphitePathPrefixProvider graphitePathPrefixProvider)
            : base($"{graphitePathPrefixProvider.GlobalPathPrefix}.SubSystem.RemoteTaskQueue.ElasticsearchIndexer", statsDClient)
        {
        }
    }
}