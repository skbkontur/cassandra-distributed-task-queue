using SKBKontur.Catalogue.Core.Configuration.Settings;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Utils;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Search
{
    public class TaskSearchDynamicSettings
    {
        public TaskSearchDynamicSettings(IApplicationSettings applicationSettings)
        {
            SearchIndexNameFormat = IndexNameConverter.ConvertToDateTimeFormat(applicationSettings.GetString("ElasticSearchSchema.MonitoringSearch.SearchIndexNameFormat"));
        }

        public string SearchIndexNameFormat { get; private set; }
    }
}