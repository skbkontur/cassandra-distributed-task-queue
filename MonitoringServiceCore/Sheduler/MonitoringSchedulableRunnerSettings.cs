using System;

using SKBKontur.Catalogue.ServiceLib.Settings;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Sheduler
{
    public class MonitoringSchedulableRunnerSettings : IMonitoringSchedulableRunnerSettings
    {
        public MonitoringSchedulableRunnerSettings(IApplicationSettings applicationSettings)
        {
            this.applicationSettings = applicationSettings;
        }

        public TimeSpan PeriodicInterval { get { return applicationSettings.GetTimeSpan("SchedulerInterval"); } }

        private readonly IApplicationSettings applicationSettings;
    }
}