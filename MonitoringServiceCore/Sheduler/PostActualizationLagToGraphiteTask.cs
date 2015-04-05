using System;

using RemoteQueue.LocalTasks.Scheduling;

using SKBKontur.Catalogue.Core.Configuration.Settings;
using SKBKontur.Catalogue.Core.Graphite.Client.Relay;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Sheduler
{
    public class PostActualizationLagToGraphiteTask : IPeriodicTask
    {
        public PostActualizationLagToGraphiteTask(
            IGraphiteRelayClientFactory graphiteRelayClientFactory,
            IApplicationSettings applicationSettings,
            IMonitoringServiceImpl monitoringServiceImpl
            )
        {
            this.applicationSettings = applicationSettings;
            this.monitoringServiceImpl = monitoringServiceImpl;
            graphiteClient = graphiteRelayClientFactory.CreateTcpClient();
        }

        public void Run()
        {
            string prefix;
            if(applicationSettings.TryGetString("RemoteTaskQueueMonitoring.GraphitePrefix", out prefix))
                graphiteClient.SendNowPoint(string.Format("{0}.ActualizationLag.{1}.Value", prefix, Environment.MachineName), (int)monitoringServiceImpl.GetActualizationLag().TotalMilliseconds);
        }

        public string Id { get { return "PostActualizationLagToGraphite"; } }
        private readonly IApplicationSettings applicationSettings;
        private readonly IMonitoringServiceImpl monitoringServiceImpl;
        private readonly ICatalogueGraphiteClient graphiteClient;
    }
}