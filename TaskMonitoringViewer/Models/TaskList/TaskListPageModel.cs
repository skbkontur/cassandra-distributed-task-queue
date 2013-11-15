using SKBKontur.Catalogue.Core.Web.Models.ModelConfigurations;
using SKBKontur.Catalogue.Core.Web.PageModels;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskList
{
    public class TaskListPageModel : PageModelBase<TaskListModelData>
    {
        public TaskListPageModel(PageModelBaseParameters parameters, WebMutatorsTree<TaskListModelData> webMutatorsTree, TaskListModelData taskListModelData)
            : base(parameters, webMutatorsTree)
        {
            Data = taskListModelData;
        }

        public override sealed TaskListModelData Data { get; protected set; }
        public TaskListPaginatorModelData PaginatorModelData { get; set; }
        public TaskListHtmlModel HtmlModel { get; set; }
    }
}