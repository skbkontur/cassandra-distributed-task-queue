using SKBKontur.Catalogue.Core.Configuration.Settings;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Utils;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Search
{
    public class TaskSearchDynamicSettings
    {
        public TaskSearchDynamicSettings(IApplicationSettings applicationSettings)
        {
            SearchIndexNameFormat = IndexNameConverter.ConvertToDateTimeFormat(
                IndexNameConverter.FillIndexNamePlaceholder(applicationSettings.GetString("ElasticSearchSchema.MonitoringSearch.SearchAliasFormat"),
                                                            applicationSettings.GetString("ElasticSearchSchema.MonitoringSearch.CurrentIndexNameFormat")));
        }

        public string SearchIndexNameFormat { get; private set; }
    }
}