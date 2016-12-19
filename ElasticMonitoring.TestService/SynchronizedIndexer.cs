using RemoteTaskQueue.Monitoring.Indexer;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TestService
{
    public class SynchronizedIndexer
    {
        public SynchronizedIndexer(IRtqElasticsearchIndexer indexer)
        {
            this.indexer = indexer;
        }

        public void ProcessNewEvents()
        {
            lock(indexer)
                indexer.ProcessNewEvents();
        }

        private readonly IRtqElasticsearchIndexer indexer;
    }
}