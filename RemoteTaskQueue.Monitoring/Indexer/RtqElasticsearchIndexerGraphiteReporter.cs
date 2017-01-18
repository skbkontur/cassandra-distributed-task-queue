using SKBKontur.Catalogue.Core.Graphite.Client.StatsD;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class RtqElasticsearchIndexerGraphiteReporter : RtqElasticsearchIndexerGraphiteReporterBase
    {
        public RtqElasticsearchIndexerGraphiteReporter(ICatalogueStatsDClient statsDClient)
            : base("EDI.SubSystem.RemoteTaskQueue.ElasticsearchIndexer", statsDClient)
        {
        }
    }
}