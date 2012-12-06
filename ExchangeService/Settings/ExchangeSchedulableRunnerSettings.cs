using System;

using RemoteQueue.Settings;

using SKBKontur.Catalogue.ServiceLib.Settings;

namespace ExchangeService.Settings
{
    public class ExchangeSchedulableRunnerSettings : IExchangeSchedulableRunnerSettings
    {
        public ExchangeSchedulableRunnerSettings(IApplicationSettings applicationSettings)
        {
            this.applicationSettings = applicationSettings;
        }

        public TimeSpan PeriodicInterval { get { return applicationSettings.GetTimeSpan("SchedulerInterval"); } }

        public int MaxRunningTasksCount { get { return applicationSettings.GetInt("MaxRunningTasksCount"); } }
        public int ShardsCount { get { return GetIntOrDefault("ShardsCount", 1); } }

        public int ShardIndex { get { return GetIntOrDefault("ShardIndex", 1); } }

        private readonly IApplicationSettings applicationSettings;

        private int GetIntOrDefault(string name, int defaultValue)
        {
            int result;
            if (!applicationSettings.TryGetInt(name, out result))
            {
                result = defaultValue;
            }
            return result;
        }
    }
}