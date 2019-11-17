using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;

using RemoteTaskQueue.Monitoring.Storage;

using SkbKontur.Cassandra.TimeBasedUuid;

using SKBKontur.Catalogue.Core.EventFeeds;

using SkbKontur.Graphite.Client;

using Vostok.Logging.Abstractions;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class RtqMonitoringEventBulkIndexer
    {
        public RtqMonitoringEventBulkIndexer(ILog logger,
                                             RtqElasticsearchIndexerSettings indexerSettings,
                                             IRtqElasticsearchClient elasticsearchClient,
                                             RemoteQueue.Handling.RemoteTaskQueue remoteTaskQueue,
                                             IStatsDClient statsDClient)
        {
            this.indexerSettings = indexerSettings;
            eventLogRepository = remoteTaskQueue.EventLogRepository;
            offsetInterpreter = new RtqEventLogOffsetInterpreter();
            var perfGraphiteReporter = new RtqMonitoringPerfGraphiteReporter("SubSystem.RemoteTaskQueue.ElasticsearchBulkIndexer", statsDClient);
            this.logger = logger.ForContext("CassandraDistributedTaskQueue").ForContext(nameof(RtqMonitoringEventBulkIndexer));
            taskMetaProcessor = new TaskMetaProcessor(this.logger, indexerSettings, elasticsearchClient, remoteTaskQueue, perfGraphiteReporter);
        }

        public void ProcessEvents([NotNull] Timestamp indexingStartTimestamp, [NotNull] Timestamp indexingFinishTimestamp)
        {
            if (indexingStartTimestamp >= indexingFinishTimestamp)
            {
                logger.Info(string.Format("IndexingFinishTimestamp is reached: {0}", indexingFinishTimestamp));
                return;
            }
            logger.Info(string.Format("Processing events from {0} to {1}", indexingStartTimestamp, indexingFinishTimestamp));
            Timestamp lastEventsBatchStartTimestamp = null;
            var taskIdsToProcess = new HashSet<string>();
            var taskIdsToProcessInChronologicalOrder = new List<string>();
            EventsQueryResult<TaskMetaUpdatedEvent, string> eventsQueryResult;
            var fromOffsetExclusive = offsetInterpreter.GetMaxOffsetForTimestamp(indexingStartTimestamp.AddTicks(-1));
            var toOffsetInclusive = offsetInterpreter.GetMaxOffsetForTimestamp(indexingFinishTimestamp);
            do
            {
                eventsQueryResult = eventLogRepository.GetEvents(fromOffsetExclusive, toOffsetInclusive, estimatedCount : 10000);
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
        private readonly EventLogRepository eventLogRepository;
        private readonly RtqEventLogOffsetInterpreter offsetInterpreter;
        private readonly TaskMetaProcessor taskMetaProcessor;
    }
}