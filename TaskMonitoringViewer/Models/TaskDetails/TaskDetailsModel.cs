using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskDetails.TaskData;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskDetails
{
    public class TaskDetailsModel
    {
        public TaskMetaInfoModel TaskMetaInfoModel { get; set; }

        public string[] ChildTaskIds { get; set; }
        public ITaskDataValue TaskDataValue { get; set; }
        public TaskExceptionInfo ExceptionInfo { get; set; }

        public int PageNumber { get; set; }
        public string SearchRequestId { get; set; }
    }
}