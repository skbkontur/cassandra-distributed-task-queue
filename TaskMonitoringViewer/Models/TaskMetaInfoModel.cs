using System;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities.Primitives;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models
{
    public class TaskMetaInfoModel
    {
        public string TaskId { get; set; }
        public TaskState State { get; set; }
        public string Name { get; set; }
        
        public DateTime? EnqueueTime { get; set; }
        public DateTime? StartExecutingTime { get; set; }
        public DateTime? FinishExecutingTime { get; set; }
        public DateTime? MinimalStartTime { get; set; }

        public int Attempts { get; set; }
        public string ParentTaskId { get; set; }
        public string TaskGroupLock { get; set; }
    }
}