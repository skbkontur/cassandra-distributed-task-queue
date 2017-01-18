using JetBrains.Annotations;

using SKBKontur.Catalogue.Objects;

namespace RemoteTaskQueue.Monitoring.Storage.Writing
{
    public interface IRtqElasticsearchIndexerProgressMarkerStorage
    {
        [CanBeNull]
        Timestamp IndexingFinishTimestamp { get; }

        [NotNull]
        Timestamp GetIndexingStartTimestamp();

        void SetIndexingStartTimestamp([NotNull] Timestamp newIndexigStartTimestamp);
    }
}