using SkbKontur.Graphite.Client;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class RtqElasticsearchIndexerGraphiteReporter : RtqElasticsearchIndexerGraphiteReporterBase
    {
        public RtqElasticsearchIndexerGraphiteReporter(IStatsDClient statsDClient)
            : base("SubSystem.RemoteTaskQueue.ElasticsearchIndexer", statsDClient)
        {
        }
    }
}