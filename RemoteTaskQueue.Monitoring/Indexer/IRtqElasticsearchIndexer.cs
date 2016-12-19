using JetBrains.Annotations;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public interface IRtqElasticsearchIndexer
    {
        void ProcessNewEvents();

        [NotNull]
        RtqElasticsearchIndexerStatus GetStatus();
    }
}