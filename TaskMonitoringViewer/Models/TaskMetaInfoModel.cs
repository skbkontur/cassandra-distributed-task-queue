using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities.Primitives;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models
{
    public class TaskMetaInfoModel
    {
        public string TaskId { get; set; }
        public TaskState State { get; set; }
        public string Name { get; set; }
        public string EnqueueTicks { get; set; }
        public string StartExecutedTicks { get; set; }
        public string MinimalStartTicks { get; set; }
        public string EnqueueMoscowTime { get; set; }
        public string StartExecutedMoscowTime { get; set; }
        public string MinimalStartMoscowTime { get; set; }
        public int Attempts { get; set; }
        public string ParentTaskId { get; set; }
    }
}