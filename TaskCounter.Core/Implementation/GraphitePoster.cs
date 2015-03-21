using System;

using SKBKontur.Catalogue.Core.Configuration.Settings;
using SKBKontur.Catalogue.Core.Graphite.Client.Relay;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation
{
    public class GraphitePoster
    {
        public GraphitePoster(IGraphiteRelayClientFactory graphiteRelayClientFactory, ICompositeCounter counter, IApplicationSettings applicationSettings)
        {
            this.counter = counter;
            graphitePrefix = applicationSettings.TryGetString("TaskCounter.GraphitePrefix", out graphitePrefix) ? graphitePrefix : null;
            graphiteRelayClient = graphiteRelayClientFactory.CreateTcpClient();
        }

        public void PostData()
        {
            if(string.IsNullOrEmpty(graphitePrefix))
                return;
            var totalCount = counter.GetTotalCount();
            var taskCounts = counter.GetAllCounts();
            //todo post time value, not now
            graphiteRelayClient.SendNowPoint(string.Format("{0}.TotalCount.TaskCounter.{1}", graphitePrefix, Environment.MachineName), totalCount.Count);
            graphiteRelayClient.SendNowPoint(string.Format("{0}.ActualizationLag.TaskCounter.{1}", graphitePrefix, Environment.MachineName), (long)TimeSpan.FromTicks(DateTime.UtcNow.Ticks - totalCount.UpdateTicks).TotalMilliseconds);
            foreach(var kvp in taskCounts)
                graphiteRelayClient.SendNowPoint(string.Format("{0}.{2}_Count.TaskCounter.{1}", graphitePrefix, Environment.MachineName, kvp.Key), kvp.Value.Count);
        }

        private readonly ICompositeCounter counter;
        private readonly IGraphiteRelayClient graphiteRelayClient;
        private readonly string graphitePrefix;
    }
}