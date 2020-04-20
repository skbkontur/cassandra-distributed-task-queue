using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.TimeBasedUuid;

using SKBKontur.Catalogue.Objects;

using SkbKontur.Graphite.Client;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TaskCounter
{
    [PublicAPI]
    public class RtqTaskCounterGraphiteReporter
    {
        public RtqTaskCounterGraphiteReporter(RtqTaskCounterStateManager stateManager, IGraphiteClient graphiteClient, [NotNull] string graphitePathPrefix)
        {
            if (string.IsNullOrEmpty(graphitePathPrefix))
                throw new InvalidProgramStateException("graphitePathPrefix is empty");
            this.stateManager = stateManager;
            this.graphiteClient = graphiteClient;
            this.graphitePathPrefix = graphitePathPrefix;
        }

        public void ReportTaskCountersToGraphite()
        {
            if (!stateManager.EventFeedIsRunning)
                return;

            var now = Timestamp.Now;
            var counters = stateManager.GetTaskCounters(now);

            graphiteClient.Send($"{graphitePathPrefix}.LostTasks", counters.LostTasksCount, now.ToDateTime());
            ReportTaskCounts($"{graphitePathPrefix}.PendingTasksTotal", counters.PendingTaskCountsTotal, now);
            foreach (var kvp in counters.PendingTaskCountsByName)
                ReportTaskCounts($"{graphitePathPrefix}.PendingTasksByName.{kvp.Key}", kvp.Value, now);
        }

        private void ReportTaskCounts([NotNull] string prefix, [NotNull] Dictionary<TaskState, int> counts, [NotNull] Timestamp now)
        {
            graphiteClient.Send($"{prefix}.CountTotal", counts.Values.Sum(), now.ToDateTime());
            foreach (var kvp in counts)
                graphiteClient.Send($"{prefix}.CountByState.{kvp.Key}", kvp.Value, now.ToDateTime());
        }

        private readonly RtqTaskCounterStateManager stateManager;
        private readonly IGraphiteClient graphiteClient;
        private readonly string graphitePathPrefix;
    }
}