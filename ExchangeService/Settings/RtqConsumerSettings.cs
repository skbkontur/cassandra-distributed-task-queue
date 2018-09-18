using System;

using RemoteQueue.Settings;

using SKBKontur.Catalogue.Core.Configuration.Settings;

namespace ExchangeService.Settings
{
    public class RtqConsumerSettings : IRtqConsumerSettings
    {
        public RtqConsumerSettings(IApplicationSettings applicationSettings)
        {
            PeriodicInterval = applicationSettings.GetTimeSpan("SchedulerInterval");
            MaxRunningTasksCount = applicationSettings.GetInt("MaxRunningTasksCount");
            MaxRunningContinuationsCount = applicationSettings.GetInt("MaxRunningContinuationsCount");
        }

        public TimeSpan PeriodicInterval { get; private set; }
        public int MaxRunningTasksCount { get; private set; }
        public int MaxRunningContinuationsCount { get; private set; }
    }
}