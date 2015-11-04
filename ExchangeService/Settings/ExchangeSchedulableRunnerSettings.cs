using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using RemoteQueue.Settings;

using SKBKontur.Catalogue.Core.Configuration.Settings;

namespace ExchangeService.Settings
{
    public class ExchangeSchedulableRunnerSettings : IExchangeSchedulableRunnerSettings
    {
        public ExchangeSchedulableRunnerSettings(IApplicationSettings applicationSettings)
        {
            Topics = null;
            PeriodicInterval = applicationSettings.GetTimeSpan("SchedulerInterval");
            MaxRunningTasksCount = applicationSettings.GetInt("MaxRunningTasksCount");
            MaxRunningContinuationsCount = applicationSettings.GetInt("MaxRunningContinuationsCount");
        }

        [CanBeNull]
        public HashSet<string> Topics { get; private set; }

        public TimeSpan PeriodicInterval { get; private set; }
        public int MaxRunningTasksCount { get; private set; }
        public int MaxRunningContinuationsCount { get; private set; }
    }
}