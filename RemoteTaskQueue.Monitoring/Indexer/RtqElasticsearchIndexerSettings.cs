using System;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class RtqElasticsearchIndexerSettings
    {
        public RtqElasticsearchIndexerSettings()
        {
            EventsReadingBatchSize = 5000;
            MaxEventsProcessingTimeWindow = TimeSpan.FromHours(72);
            MaxEventsProcessingTasksCount = 10 * 1000 * 1000;
            TaskIdsProcessingBatchSize = 4000;
            IndexingThreadsCount = 16;
        }

        public int EventsReadingBatchSize { get; set; }
        public TimeSpan MaxEventsProcessingTimeWindow { get; set; }
        public int MaxEventsProcessingTasksCount { get; set; }
        public int TaskIdsProcessingBatchSize { get; set; }
        public int IndexingThreadsCount { get; set; }

        public override string ToString()
        {
            return string.Format("EventsReadingBatchSize: {0}, MaxEventsProcessingTimeWindow: {1}, MaxEventsProcessingTasksCount: {2}, TaskIdsProcessingBatchSize: {3}, IndexingThreadsCount: {4}",
                                 EventsReadingBatchSize, MaxEventsProcessingTimeWindow, MaxEventsProcessingTasksCount, TaskIdsProcessingBatchSize, IndexingThreadsCount);
        }
    }
}