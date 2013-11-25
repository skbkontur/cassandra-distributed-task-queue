using SKBKontur.Catalogue.Core.Web.PageModels;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskDetails
{
    public class TaskDetailsPageModel : PageModelBase<TaskDetailsModel>
    {
        public TaskDetailsPageModel(PageModelBaseParameters parameters, TaskDetailsModel modelData)
            : base(parameters, null)
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
}