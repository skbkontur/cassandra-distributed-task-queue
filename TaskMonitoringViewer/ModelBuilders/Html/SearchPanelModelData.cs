using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities.Primitives;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.Html
{
    public class SearchPanelModelData
    {
        public string TaskName { get; set; }
        public Pair<TaskState, bool?> [] States {get; set;}
        public string[] AllowedTaskNames { get; set; }
    }
}