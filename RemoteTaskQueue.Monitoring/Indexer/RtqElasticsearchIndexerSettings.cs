using System;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Objects;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class RtqElasticsearchIndexerSettings
    {
        public RtqElasticsearchIndexerSettings()
        {
            InitialIndexingStartTimestamp = new Timestamp(new DateTime(2016, 02, 01, 0, 0, 0, DateTimeKind.Utc));
            MaxEventsProcessingTimeWindow = TimeSpan.FromHours(1);
            MaxEventsProcessingTasksCount = 60000; // slightly less than TaskIdsProcessingBatchSize * IndexingThreadsCount
            TaskIdsProcessingBatchSize = 4000;
            IndexingThreadsCount = 16;
        }

        [NotNull]
        public Timestamp InitialIndexingStartTimestamp { get; private set; }

        public TimeSpan MaxEventsProcessingTimeWindow { get; set; }
        public int MaxEventsProcessingTasksCount { get; set; }
        public int TaskIdsProcessingBatchSize { get; set; }
        public int IndexingThreadsCount { get; set; }

        public override string ToString()
        {
            return string.Format("InitialIndexingStartTimestamp: {0}, MaxEventsProcessingTimeWindow: {1}, MaxEventsProcessingTasksCount: {2}, TaskIdsProcessingBatchSize: {3}, IndexingThreadsCount: {4}",
                                 InitialIndexingStartTimestamp, MaxEventsProcessingTimeWindow, MaxEventsProcessingTasksCount, TaskIdsProcessingBatchSize, IndexingThreadsCount);
        }
    }
}