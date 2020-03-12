using System;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

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

        public TimeSpan PeriodicInterval { get; }
        public int MaxRunningTasksCount { get; }
        public int MaxRunningContinuationsCount { get; }
    }
}