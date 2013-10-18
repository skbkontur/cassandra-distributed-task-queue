using SKBKontur.Catalogue.Core.Web.Models.DateAndTimeModels;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities.Primitives;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models
{
    public class TaskMetaInfoModel
    {
        public string TaskId { get; set; }
        public TaskState State { get; set; }
        public string Name { get; set; }

        public DateAndTime EnqueueTime { get; set; }
        public DateAndTime StartExecutingTime { get; set; }
        public DateAndTime FinishExecutingTime { get; set; }
        public DateAndTime MinimalStartTime { get; set; }

        public int Attempts { get; set; }
        public string ParentTaskId { get; set; }
        public string TaskGroupLock { get; set; }
    }
}