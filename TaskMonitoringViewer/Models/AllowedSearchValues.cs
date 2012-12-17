using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities.Primitives;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models
{
    public class AllowedSearchValues
    {
        public string[] Names { get; set; }
        public TaskState[] States { get; set; }
    }
}