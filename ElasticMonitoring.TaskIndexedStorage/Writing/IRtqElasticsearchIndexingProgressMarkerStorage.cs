using JetBrains.Annotations;

using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing
{
    public interface IRtqElasticsearchIndexingProgressMarkerStorage
    {
        [CanBeNull]
        Timestamp IndexingFinishTimestamp { get; }

        long GetLastReadTicks();
        void SetLastReadTicks(long ticks);
    }
}