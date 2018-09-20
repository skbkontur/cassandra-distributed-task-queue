using SkbKontur.Graphite.Client;

using SKBKontur.Catalogue.ServiceLib.Graphite;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class RtqElasticsearchIndexerGraphiteReporter : RtqElasticsearchIndexerGraphiteReporterBase
    {
        public RtqElasticsearchIndexerGraphiteReporter(IStatsDClient statsDClient, IGraphitePathPrefixProvider graphitePathPrefixProvider)
            : base($"{graphitePathPrefixProvider.GlobalPathPrefix}.SubSystem.RemoteTaskQueue.ElasticsearchIndexer", statsDClient)
        {
        }
    }
}