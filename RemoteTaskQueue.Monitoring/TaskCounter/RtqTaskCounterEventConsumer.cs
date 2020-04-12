using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories;
using SkbKontur.EventFeeds;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TaskCounter
{
    public class RtqTaskCounterEventConsumer : IEventConsumer<TaskMetaUpdatedEvent, string>
    {
        public RtqTaskCounterEventConsumer(RtqTaskCounterStateManager stateManager,
                                           IHandleTasksMetaStorage handleTasksMetaStorage,
                                           RtqMonitoringPerfGraphiteReporter perfGraphiteReporter)
        {
            this.stateManager = stateManager;
            this.handleTasksMetaStorage = handleTasksMetaStorage;
            this.perfGraphiteReporter = perfGraphiteReporter;
        }

        [NotNull]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public string GetDescription()
        {
            return GetType().FullName;
        }

        public void ResetLocalState()
        {
            stateManager.ResetLocalState();
        }

        [NotNull]
        public EventsProcessingResult<string> ProcessEvents([NotNull] EventsQueryResult<TaskMetaUpdatedEvent, string> eventsQueryResult)
        {
            stateManager.MaybePersistState();

            var taskIdsToProcess = new HashSet<string>();
            foreach (var @event in eventsQueryResult.Events)
            {
                if (stateManager.NeedToProcessEvent(@event.Event))
                    taskIdsToProcess.Add(@event.Event.TaskId);
            }

            perfGraphiteReporter.Increment("TaskMetasToRead", taskIdsToProcess.Count);
            var taskMetas = perfGraphiteReporter.ReportTiming("ReadTaskMetas", () => handleTasksMetaStorage.GetMetas(taskIdsToProcess.ToArray()));
            perfGraphiteReporter.Increment("MissedTaskMetas", taskIdsToProcess.Count - taskMetas.Count);

            foreach (var taskMeta in taskMetas)
                stateManager.UpdateTaskState(taskMeta.Value);

            return eventsQueryResult.LastOffset == null
                       ? EventsProcessingResult<string>.DoNotCommitOffset()
                       : EventsProcessingResult<string>.DoCommitOffset(eventsQueryResult.LastOffset);
        }

        private readonly RtqTaskCounterStateManager stateManager;
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly RtqMonitoringPerfGraphiteReporter perfGraphiteReporter;
    }
}