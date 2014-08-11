using System;

using SKBKontur.Catalogue.Core.Configuration.Settings;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Settings
{
    public class MonitoringServiceSettings : IMonitoringServiceSettings
    {
        public MonitoringServiceSettings(IApplicationSettings applicationSettings)
        {
            PeriodicInterval = applicationSettings.GetTimeSpan("SchedulerInterval");
            bool actualizeOnQuery;
            if(!applicationSettings.TryGetBool("ActualizeOnQuery", out actualizeOnQuery))
                actualizeOnQuery = false;
            ActualizeOnQuery = actualizeOnQuery;
        }

        public TimeSpan PeriodicInterval { get; private set; }
        public bool ActualizeOnQuery { get; private set; }
    }
}