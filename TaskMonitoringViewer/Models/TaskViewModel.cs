using RemoteQueue.Cassandra.Entities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models
{
    public class TaskViewModel
    {
        public TaskMetaInfoModel TaskMetaInfoModel { get; set; }
        public ITaskDataValue TaskDataValue { get; set; }
        public TaskExceptionInfo ExceptionInfo { get; set; }
        public int? PageNumber { get; set; }
        public string SearchRequestId { get; set; }
    }
}