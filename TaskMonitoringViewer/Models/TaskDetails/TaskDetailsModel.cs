using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.Core.Web.PageModels;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskDetails
{
    public class TaskDetailsModel : ModelData
    {
        public TaskMetaInfoModel TaskMetaInfoModel { get; set; }

        public string[] ChildTaskIds { get; set; }
        public ITaskData TaskData { get; set; }
        public TaskExceptionInfo ExceptionInfo { get; set; }
    }
}