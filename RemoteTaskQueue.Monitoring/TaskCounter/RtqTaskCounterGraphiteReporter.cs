using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.Objects;

using SkbKontur.Graphite.Client;

namespace RemoteTaskQueue.Monitoring.TaskCounter
{
    public class RtqTaskCounterGraphiteReporter
    {
        public RtqTaskCounterGraphiteReporter(RtqTaskCounterStateManager stateManager, IGraphiteClient graphiteClient)
        {
            this.stateManager = stateManager;
            this.graphiteClient = graphiteClient;
            graphitePrefix = "SubSystem.RemoteTaskQueue.TaskCounter";
        }

        public void ReportTaskCountersToGraphite()
        {
            if (!stateManager.EventFeedIsRunning)
                return;

            var now = Timestamp.Now;
            var counters = stateManager.GetTaskCounters(now);

            graphiteClient.Send($"{graphitePrefix}.LostTasks", counters.LostTasksCount, now.ToDateTime());
            ReportTaskCounts($"{graphitePrefix}.PendingTasksTotal", counters.PendingTaskCountsTotal, now);
            foreach (var kvp in counters.PendingTaskCountsByName)
                ReportTaskCounts($"{graphitePrefix}.PendingTasksByName.{kvp.Key}", kvp.Value, now);
        }

        private void ReportTaskCounts([NotNull] string prefix, [NotNull] Dictionary<TaskState, int> counts, [NotNull] Timestamp now)
        {
            graphiteClient.Send($"{prefix}.CountTotal", counts.Values.Sum(), now.ToDateTime());
            foreach (var kvp in counts)
                graphiteClient.Send($"{prefix}.CountByState.{kvp.Key}", kvp.Value, now.ToDateTime());
        }

        private readonly RtqTaskCounterStateManager stateManager;
        private readonly IGraphiteClient graphiteClient;
        private readonly string graphitePrefix;
    }
}