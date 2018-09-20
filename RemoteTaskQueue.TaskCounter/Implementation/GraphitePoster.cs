using System;

using RemoteQueue.Cassandra.Entities;

using SkbKontur.Graphite.Client;

using SKBKontur.Catalogue.ServiceLib.Graphite;

namespace RemoteTaskQueue.TaskCounter.Implementation
{
    public class GraphitePoster
    {
        public GraphitePoster(IGraphiteClient graphiteClient, IGraphitePathPrefixProvider graphitePathPrefixProvider, ICompositeCounter counter)
        {
            this.counter = counter;
            graphitePrefix = $"{graphitePathPrefixProvider.GlobalPathPrefix}.SubSystem.RemoteTaskQueueCounter";
            this.graphiteClient = graphiteClient;
        }

        public void PostData()
        {
            if (string.IsNullOrEmpty(graphitePrefix))
                return;
            var totalCount = counter.GetTotalCount();
            var taskCounts = counter.GetAllCounts();
            //todo post time value, not now
            DateTime utcNow = DateTime.UtcNow;
            graphiteClient.Send($"{graphitePrefix}.TotalCount.OldWaitingTaskCount.{Environment.MachineName}", totalCount.OldWaitingTaskCount, utcNow);
            graphiteClient.Send($"{graphitePrefix}.TotalCount.TaskCounter.{Environment.MachineName}", totalCount.Count, utcNow);
            graphiteClient.Send($"{graphitePrefix}.ActualizationLag.TaskCounter.{Environment.MachineName}", (long)TimeSpan.FromTicks(utcNow.Ticks - totalCount.UpdateTicks).TotalMilliseconds, utcNow);
            SendCountsByState($"{graphitePrefix}.TotalCount", totalCount.Counts);
            foreach (var kvp in taskCounts)
            {
                graphiteClient.Send($"{graphitePrefix}.{kvp.Key}_Count.TaskCounter.{Environment.MachineName}", kvp.Value.Count, utcNow);
                SendCountsByState($"{graphitePrefix}.{kvp.Key}_Count", kvp.Value.Counts);
            }
        }

        private void SendCountsByState(string prefix, int[] counts)
        {
            var states = new[] {TaskState.Unknown, TaskState.New, TaskState.InProcess, TaskState.WaitingForRerun, TaskState.WaitingForRerunAfterError};
            foreach (var taskState in states)
            {
                var count = counts[(int)taskState];
                graphiteClient.Send($"{prefix}.{taskState}.{Environment.MachineName}", count, DateTime.UtcNow);
            }
        }

        private readonly IGraphiteClient graphiteClient;
        private readonly ICompositeCounter counter;
        private readonly string graphitePrefix;
    }
}