using SKBKontur.Catalogue.Core.Configuration.Settings;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Search
{
    public class TaskSearchDynamicSettings
    {
        public TaskSearchDynamicSettings(IApplicationSettings applicationSettings)
        {
            SearchIndexNameFormat = applicationSettings.GetString("ElasticSearchSchema.MonitoringSearch.SearchIndexNameFormat");
        }

        public string SearchIndexNameFormat { get; private set; }
    }
}