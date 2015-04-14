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
        }

        public bool EnableDestructiveActions { get { return enableDestructiveActions; } }

        public string CurrentIndexNameFormat { get; private set; }
        public string OldIndexNameFormat { get; private set; }
        public string LastTicksIndex { get; private set; }

        private readonly bool enableDestructiveActions;
    }
}