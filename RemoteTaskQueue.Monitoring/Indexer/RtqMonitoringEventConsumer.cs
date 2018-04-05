using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.Core.EventFeeds;
using SKBKontur.Catalogue.Objects;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class RtqMonitoringEventConsumer : IEventConsumer<TaskMetaUpdatedEvent, string>
    {
        public RtqMonitoringEventConsumer(RtqElasticsearchIndexerSettings settings, TaskMetaProcessor taskMetaProcessor)
        {
            this.settings = settings;
            this.taskMetaProcessor = taskMetaProcessor;
        }

        [NotNull]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public string GetDescription()
        {
            return GetType().FullName;
        }

        public void ResetLocalState()
        {
            taskIdsToProcess = new HashSet<string>();
            taskIdsToProcessInChronologicalOrder = new List<string>();
            lastNotProcessedEventsBatchStartTimestamp = null;
        }

        [NotNull]
        public EventsProcessingResult<string> ProcessEvents([NotNull] EventsQueryResult<TaskMetaUpdatedEvent, string> eventsQueryResult)
        {
            string offsetToCommit = null;
            foreach (var @event in eventsQueryResult.Events)
            {
                if (taskIdsToProcess.Add(@event.Event.TaskId))
                    taskIdsToProcessInChronologicalOrder.Add(@event.Event.TaskId);
                var eventTimestamp = new Timestamp(@event.Event.Ticks);
                if (lastNotProcessedEventsBatchStartTimestamp == null)
                    lastNotProcessedEventsBatchStartTimestamp = eventTimestamp;
                if (eventTimestamp - lastNotProcessedEventsBatchStartTimestamp > settings.MaxEventsProcessingTimeWindow || taskIdsToProcessInChronologicalOrder.Count > settings.MaxEventsProcessingTasksCount)
                {
                    IndexTasks();
                    offsetToCommit = @event.Offset;
                }
            }
            if (eventsQueryResult.NoMoreEventsInSource)
            {
                if (taskIdsToProcessInChronologicalOrder.Any())
                    IndexTasks();
                offsetToCommit = eventsQueryResult.LastOffset;
            }
            return offsetToCommit != null ? EventsProcessingResult<string>.DoCommitOffset(offsetToCommit) : EventsProcessingResult<string>.DoNotCommitOffset();
        }

        private void IndexTasks()
        {
            taskMetaProcessor.ProcessTasks(taskIdsToProcessInChronologicalOrder);
            taskIdsToProcess.Clear();
            taskIdsToProcessInChronologicalOrder.Clear();
            lastNotProcessedEventsBatchStartTimestamp = null;
        }

        [ThreadStatic]
        private static HashSet<string> taskIdsToProcess;

        [ThreadStatic]
        private static List<string> taskIdsToProcessInChronologicalOrder;

        [ThreadStatic]
        private static Timestamp lastNotProcessedEventsBatchStartTimestamp;

        private readonly RtqElasticsearchIndexerSettings settings;
        private readonly TaskMetaProcessor taskMetaProcessor;
    }
}