using SKBKontur.Catalogue.Core.ObjectTreeWebViewer.Models;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Primitives;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskDetails
{
    public class TaskDetailsHtmlModel
    {
        public TaskMetaInfoHtmlModel TaskMetaInfo { get; set; }
        public TaskIdHtmlModel[] ChildTaskIds { get; set; }
        public IObjectTreeValue TaskDataValue { get; set; }
        public ExceptionInfoHtmlModel ExceptionInfo { get; set; }
    }
}