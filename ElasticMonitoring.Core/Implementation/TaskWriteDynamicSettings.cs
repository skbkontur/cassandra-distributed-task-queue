using System;
using System.Globalization;

using SKBKontur.Catalogue.Core.Configuration.Settings;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Utils;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation
{
    public class TaskWriteDynamicSettings : ITaskWriteDynamicSettings
    {
        public TaskWriteDynamicSettings(IApplicationSettings applicationSettings)
        {
            if(!applicationSettings.TryGetBool("ElasticSearchSchema.MonitoringSearch.EnableDestructveActions", out enableDestructiveActions))
                enableDestructiveActions = false;
            CurrentIndexNameFormat = IndexNameConverter.ConvertToDateTimeFormat(applicationSettings.GetString("ElasticSearchSchema.MonitoringSearch.CurrentIndexNameFormat"));
            OldIndexNameFormat = IndexNameConverter.ConvertToDateTimeFormat(applicationSettings.GetString("ElasticSearchSchema.MonitoringSearch.OldIndexNameFormat"));
            LastTicksIndex = applicationSettings.GetString("ElasticSearchSchema.MonitoringSearch.LastTicksIndex");

            string timeStr;
            CalculatedIndexStartTimeTicks = !applicationSettings.TryGetString("ElasticSearchSchema.MonitoringSearch.IndexStartTime", out timeStr) ? 0 : ConvertToUtcTicks(timeStr);
            string value;
            GraphitePrefixOrNull = !applicationSettings.TryGetString("ElasticMonitoring.GraphitePrefix", out value) ? null : value;
            RemoteLockId = applicationSettings.GetString("ElasticMonitoring.RemoteLockId");

            long maxTicks;
            MaxTicks = !applicationSettings.TryGetLong("ElasticSearchSchema.MonitoringSearch.MaxTicks", out maxTicks) ? null : (long?)maxTicks;
        }

        private static long ConvertToUtcTicks(string timeStr)
        {
            if(string.IsNullOrEmpty(timeStr))
                return 0;
            const string nowConst = "{now}";
            if(timeStr.StartsWith(nowConst))
            {
                timeStr = timeStr.Substring(nowConst.Length, timeStr.Length - nowConst.Length);
                var timeSpan = TimeSpan.Parse(timeStr);
                if(timeSpan.Ticks > 0)
                    throw new NotSupportedException("IndexStartTime should have negative offset from {now}");
                return (DateTime.UtcNow + timeSpan).ToUniversalTime().Ticks;
            }
            DateTime time;
            if(!DateTime.TryParse(timeStr, CultureInfo.GetCultureInfo("ru-RU"), DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out time))
                throw new NotSupportedException(string.Format("'{0}' has incorrect DateTime format", timeStr));
            return time.ToUniversalTime().Ticks;
        }

        public bool EnableDestructiveActions { get { return enableDestructiveActions; } }

        public string CurrentIndexNameFormat { get; private set; }
        public string OldIndexNameFormat { get; private set; }
        public string LastTicksIndex { get; private set; }
        public long CalculatedIndexStartTimeTicks { get; private set; }

        public string GraphitePrefixOrNull { get; private set; }
        public string RemoteLockId { get; private set; }
        public long? MaxTicks { get; private set; }

        private readonly bool enableDestructiveActions;
    }
}