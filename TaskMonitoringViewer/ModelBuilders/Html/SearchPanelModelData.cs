using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities.Primitives;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.Html
{
    public class SearchPanelModelData
    {
        public string TaskName { get; set; }
        public Pair<TaskState, bool?> [] States {get; set;}
        public string TaskId { get; set; }
        public string ParentTaskId { get; set; }
        public string[] AllowedTaskNames { get; set; }
        public DateTimeRangeModel Ticks { get; set; }
        public DateTimeRangeModel StartExecutedTicks { get; set; }
        public DateTimeRangeModel MinimalStartTicks { get; set; }
    }
}