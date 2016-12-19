using System;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Objects;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class RtqElasticsearchIndexerStatus
    {
        public TimeSpan ActualizationLag { get; set; }

        [CanBeNull]
        public Timestamp IndexingFinishTimestamp { get; set; }

        [NotNull]
        public Timestamp LastIndexingStartTimestamp { get; set; }
    }
}