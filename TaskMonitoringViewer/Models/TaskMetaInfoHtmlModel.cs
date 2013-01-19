using SKBKontur.Catalogue.Core.Web.Models.HtmlModels;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Primitives;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models
{
    public class TaskMetaInfoHtmlModel
    {
        public TaskIdHtmlModel TaskId { get; set; }
        public TaskStateHtmlModel TaskState { get; set; }
        public TextHtmlModel TaskName { get; set; }
        public TaskDateTimeHtmlModel EnqueueTime { get; set; }
        public TaskDateTimeHtmlModel StartExecutingTime { get; set; }
        public TaskDateTimeHtmlModel FinishExecutingTime { get; set; }
        public TaskDateTimeHtmlModel MinimalStartTime { get; set; }
        public TextHtmlModel Attempts { get; set; }
        public TaskIdHtmlModel ParentTaskId { get; set; }
        public TextHtmlModel TaskGroupLock { get; set; }
    }
}