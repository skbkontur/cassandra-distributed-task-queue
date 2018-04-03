using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;

using RemoteTaskQueue.Monitoring.Storage.Utils;

using SKBKontur.Catalogue.Core.EventFeeds;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.ServiceLib.Logging;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class RtqMonitoringEventBulkIndexer
    {
        public RtqMonitoringEventBulkIndexer(RtqElasticsearchIndexerSettings settings, EventLogRepository eventLogRepository, RtqMonitoringOffsetInterpreter offsetInterpreter, TaskMetaProcessor taskMetaProcessor)
        {
            this.settings = settings;
            this.eventLogRepository = eventLogRepository;
            this.offsetInterpreter = offsetInterpreter;
            this.taskMetaProcessor = taskMetaProcessor;
        }

        public void ProcessEvents([NotNull] Timestamp indexingStartTimestamp, [NotNull] Timestamp indexingFinishTimestamp)
        {
            if (indexingStartTimestamp >= indexingFinishTimestamp)
            {
                Log.For(this).LogInfoFormat(string.Format("IndexingFinishTimestamp is reached: {0}", indexingFinishTimestamp));
                return;
            }
            Log.For(this).LogInfoFormat("Processing events from {0} to {1}", indexingStartTimestamp, indexingFinishTimestamp);
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
                    if (eventTimestamp - lastEventsBatchStartTimestamp > settings.MaxEventsProcessingTimeWindow || taskIdsToProcessInChronologicalOrder.Count > settings.MaxEventsProcessingTasksCount)
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

        private readonly RtqElasticsearchIndexerSettings settings;
        private readonly EventLogRepository eventLogRepository;
        private readonly RtqMonitoringOffsetInterpreter offsetInterpreter;
        private readonly TaskMetaProcessor taskMetaProcessor;
    }
}