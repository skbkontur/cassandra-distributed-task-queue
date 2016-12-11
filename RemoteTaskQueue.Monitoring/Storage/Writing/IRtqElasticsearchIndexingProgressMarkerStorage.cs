using JetBrains.Annotations;

using SKBKontur.Catalogue.Objects;

namespace RemoteTaskQueue.Monitoring.Storage.Writing
{
    public interface IRtqElasticsearchIndexingProgressMarkerStorage
    {
        [CanBeNull]
        Timestamp IndexingFinishTimestamp { get; }

        long GetLastReadTicks();
        void SetLastReadTicks(long ticks);
    }
}