using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.EventFeed;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage;
using SkbKontur.Cassandra.TimeBasedUuid;
using SkbKontur.EventFeeds;
using SkbKontur.Graphite.Client;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Indexer
{
    public class RtqMonitoringEventBulkIndexer
    {
        public RtqMonitoringEventBulkIndexer(ILog logger,
                                             RtqElasticsearchIndexerSettings indexerSettings,
                                             IRtqElasticsearchClient elasticsearchClient,
                                             RemoteTaskQueue remoteTaskQueue,
                                             IStatsDClient statsDClient)
        {
            this.indexerSettings = indexerSettings;
            eventSource = new RtqEventSource(remoteTaskQueue.EventLogRepository);
            offsetInterpreter = new RtqEventLogOffsetInterpreter();
            var perfGraphiteReporter = new RtqMonitoringPerfGraphiteReporter(indexerSettings.PerfGraphitePathPrefix, statsDClient);
            this.logger = logger.ForContext("CassandraDistributedTaskQueue").ForContext(nameof(RtqMonitoringEventBulkIndexer));
            taskMetaProcessor = new TaskMetaProcessor(this.logger, indexerSettings, elasticsearchClient, remoteTaskQueue, perfGraphiteReporter);
        }

        public void ProcessEvents([NotNull] Timestamp indexingStartTimestamp, [NotNull] Timestamp indexingFinishTimestamp)
        {
            if (indexingStartTimestamp >= indexingFinishTimestamp)
            {
                logger.Info("IndexingFinishTimestamp is reached: {IndexingFinishTimestamp}", new {IndexingFinishTimestamp = indexingFinishTimestamp});
                return;
            }
            logger.Info("Processing events from {IndexingStartTimestamp} to {IndexingFinishTimestamp}",
                        new {IndexingStartTimestamp = indexingStartTimestamp, IndexingFinishTimestamp = indexingFinishTimestamp});
            Timestamp lastEventsBatchStartTimestamp = null;
            var taskIdsToProcess = new HashSet<string>();
            var taskIdsToProcessInChronologicalOrder = new List<string>();
            EventsQueryResult<TaskMetaUpdatedEvent, string> eventsQueryResult;
            var fromOffsetExclusive = offsetInterpreter.GetMaxOffsetForTimestamp(indexingStartTimestamp.AddTicks(-1));
            var toOffsetInclusive = offsetInterpreter.GetMaxOffsetForTimestamp(indexingFinishTimestamp);
            do
            {
                eventsQueryResult = eventSource.GetEvents(fromOffsetExclusive, toOffsetInclusive, estimatedCount : 10000);
                foreach (var @event in eventsQueryResult.Events)
                {
                    if (taskIdsToProcess.Add(@event.Event.TaskId))
                        taskIdsToProcessInChronologicalOrder.Add(@event.Event.TaskId);
                    var eventTimestamp = new Timestamp(@event.Event.Ticks);
                    if (lastEventsBatchStartTimestamp == null)
                        lastEventsBatchStartTimestamp = eventTimestamp;
                    if (eventTimestamp - lastEventsBatchStartTimestamp > indexerSettings.MaxEventsProcessingTimeWindow || taskIdsToProcessInChronologicalOrder.Count > indexerSettings.MaxEventsProcessingTasksCount)
                    {
                        taskMetaProcessor.ProcessTasks(taskIdsToProcessInChronologicalOrder);
                        taskIdsToProcess.Clear();
                        taskIdsToProcessInChronologicalOrder.Clear();
                        lastEventsBatchStartTimestamp = null;
                    }
                }
                fromOffsetExclusive = eventsQueryResult.LastOffset;
            } while (!eventsQueryResult.NoMoreEventsInSource);
            if (taskIdsToProcessInChronologicalOrder.Any())
                taskMetaProcessor.ProcessTasks(taskIdsToProcessInChronologicalOrder);
        }

        private readonly ILog logger;
        private readonly RtqElasticsearchIndexerSettings indexerSettings;
        private readonly RtqEventSource eventSource;
        private readonly RtqEventLogOffsetInterpreter offsetInterpreter;
        private readonly TaskMetaProcessor taskMetaProcessor;
    }
}