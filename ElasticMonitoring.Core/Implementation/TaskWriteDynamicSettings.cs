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
            const string monitoringsearch = "ElasticSearchSchema.MonitoringSearch.";
            if(!applicationSettings.TryGetBool(monitoringsearch + "EnableDestructveActions", out enableDestructiveActions))
                enableDestructiveActions = false;
            CurrentIndexNameFormat = IndexNameConverter.ConvertToDateTimeFormat(applicationSettings.GetString(monitoringsearch + "CurrentIndexNameFormat"));
            OldIndexNameFormat = IndexNameConverter.ConvertToDateTimeFormat(IndexNameConverter.FillIndexNamePlaceholder(
                applicationSettings.GetString(monitoringsearch + "OldDataAliasFormat"),
                applicationSettings.GetString(monitoringsearch + "CurrentIndexNameFormat")));
            LastTicksIndex = applicationSettings.GetString(monitoringsearch + "LastTicksIndex");
            OldDataIndex = applicationSettings.GetString(monitoringsearch + "OldDataIndex");
            string timeStr;
            CalculatedIndexStartTimeTicks = !applicationSettings.TryGetString(monitoringsearch + "IndexStartTime", out timeStr) ? 0 : ConvertToUtcTicks(timeStr);

            SearchAliasFormat = applicationSettings.GetString(monitoringsearch + "SearchAliasFormat");
            OldDataAliasFormat = applicationSettings.GetString(monitoringsearch + "OldDataAliasFormat");

            long maxTicks;
            MaxTicks = !applicationSettings.TryGetLong(monitoringsearch + "MaxTicks", out maxTicks) ? null : (long?)maxTicks;

            string value;
            GraphitePrefixOrNull = !applicationSettings.TryGetString("ElasticMonitoring.GraphitePrefix", out value) ? null : value;
            RemoteLockId = applicationSettings.GetString("ElasticMonitoring.RemoteLockId");
            MaxBatch = applicationSettings.GetInt("ElasticMonitoring.MaxBatch");
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
            if(timeStr.StartsWith("6") && timeStr.Length == 18)
                return long.Parse(timeStr);
            DateTime time;
            if(!DateTime.TryParse(timeStr, CultureInfo.GetCultureInfo("ru-RU"), DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out time))
                throw new NotSupportedException(string.Format("'{0}' has incorrect DateTime format", timeStr));
            return time.ToUniversalTime().Ticks;
        }

        public bool EnableDestructiveActions { get { return enableDestructiveActions; } }

        public string SearchAliasFormat { get; private set; }
        public string OldDataAliasFormat { get; private set; }

        public string OldDataIndex { get; private set; }
        public string CurrentIndexNameFormat { get; private set; }
        //NOTE this is alias name, not index name
        public string OldIndexNameFormat { get; private set; }
        public string LastTicksIndex { get; private set; }
        public long CalculatedIndexStartTimeTicks { get; private set; }

        public string GraphitePrefixOrNull { get; private set; }
        public string RemoteLockId { get; private set; }
        public long? MaxTicks { get; private set; }
        public int MaxBatch { get; private set; }

        private readonly bool enableDestructiveActions;
    }
}