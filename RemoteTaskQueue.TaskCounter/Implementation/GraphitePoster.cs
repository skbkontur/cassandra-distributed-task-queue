using System;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.Core.Configuration.Settings;
using SKBKontur.Catalogue.Core.Graphite.Client.Relay;

namespace RemoteTaskQueue.TaskCounter.Implementation
{
    public class GraphitePoster
    {
        public GraphitePoster(ICatalogueGraphiteClient graphiteClient, ICompositeCounter counter, IApplicationSettings applicationSettings)
        {
            this.counter = counter;
            graphitePrefix = applicationSettings.TryGetString("TaskCounter.GraphitePrefix", out graphitePrefix) ? graphitePrefix : null;
            this.graphiteClient = graphiteClient;
        }

        public void PostData()
        {
            if(string.IsNullOrEmpty(graphitePrefix))
                return;
            var totalCount = counter.GetTotalCount();
            var taskCounts = counter.GetAllCounts();
            //todo post time value, not now
            DateTime utcNow = DateTime.UtcNow;
            graphiteClient.Send(string.Format("{0}.TotalCount.OldWaitingTaskCount.{1}", graphitePrefix, Environment.MachineName), totalCount.OldWaitingTaskCount, utcNow);
            graphiteClient.Send(string.Format("{0}.TotalCount.TaskCounter.{1}", graphitePrefix, Environment.MachineName), totalCount.Count, utcNow);
            graphiteClient.Send(string.Format("{0}.ActualizationLag.TaskCounter.{1}", graphitePrefix, Environment.MachineName), (long)TimeSpan.FromTicks(utcNow.Ticks - totalCount.UpdateTicks).TotalMilliseconds, utcNow);
            SendCountsByState(string.Format("{0}.TotalCount", graphitePrefix), totalCount.Counts);
            foreach(var kvp in taskCounts)
            {
                graphiteClient.Send(string.Format("{0}.{2}_Count.TaskCounter.{1}", graphitePrefix, Environment.MachineName, kvp.Key), kvp.Value.Count, utcNow);
                SendCountsByState(string.Format("{0}.{1}_Count", graphitePrefix, kvp.Key), kvp.Value.Counts);
            }
        }

        private void SendCountsByState(string prefix, int[] counts)
        {
            var states = new[] {TaskState.Unknown, TaskState.New, TaskState.InProcess, TaskState.WaitingForRerun, TaskState.WaitingForRerunAfterError};
            foreach(var taskState in states)
            {
                var count = counts[(int)taskState];
                graphiteClient.Send(string.Format("{0}.{1}.{2}", prefix, taskState, Environment.MachineName), count, DateTime.UtcNow);
            }
        }

        private readonly ICatalogueGraphiteClient graphiteClient;
        private readonly ICompositeCounter counter;
        private readonly string graphitePrefix;
    }
}