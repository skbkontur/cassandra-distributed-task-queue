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
            ICatalogueGraphiteClient graphiteClient,
            IApplicationSettings applicationSettings,
            IMonitoringServiceImpl monitoringServiceImpl
            )
        {
            this.graphiteClient = graphiteClient;
            this.applicationSettings = applicationSettings;
            this.monitoringServiceImpl = monitoringServiceImpl;
        }

        public void Run()
        {
            string prefix;
            if(applicationSettings.TryGetString("RemoteTaskQueueMonitoring.GraphitePrefix", out prefix))
                graphiteClient.Send(string.Format("{0}.ActualizationLag.{1}.Value", prefix, Environment.MachineName), (int)monitoringServiceImpl.GetActualizationLag().TotalMilliseconds, DateTime.UtcNow);
        }

        public string Id { get { return "PostActualizationLagToGraphite"; } }
        private readonly IApplicationSettings applicationSettings;
        private readonly IMonitoringServiceImpl monitoringServiceImpl;
        private readonly ICatalogueGraphiteClient graphiteClient;
    }
}