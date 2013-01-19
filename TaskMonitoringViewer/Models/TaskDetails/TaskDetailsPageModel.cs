using System.Collections.Generic;

using SKBKontur.Catalogue.Core.Web.PageModels;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Primitives;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskDetails.TaskData;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskDetails
{
    public class TaskDetailsPageModel : PageModelBase<TaskDetailsModel>
    {
        public TaskDetailsPageModel(PageModelBaseParameters parameters, TaskDetailsModel modelData)
            : base(parameters)
        {
            Data = modelData;
        }

        public override sealed TaskDetailsModel Data { get; protected set; }
        public TaskDetailsHtmlModel HtmlModel { get; set; }

        public int PageNumber { get; set; }
        public string SearchRequestId { get; set; }
        public string Title { get; set; }
        public string TaskListUrl { get; set; }
    }

    public class TaskDetailsHtmlModel
    {
        public TaskMetaInfoHtmlModel TaskMetaInfo { get; set; }
        public TaskIdHtmlModel[] ChildTaskIds { get; set; }
        public ITaskDataValue TaskDataValue { get; set; }
        public ExceptionInfoHtmlModel ExceptionInfo { get; set; }
    }
}