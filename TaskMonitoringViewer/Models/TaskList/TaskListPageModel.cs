using SKBKontur.Catalogue.Core.Web.Models.ModelConfigurations;
using SKBKontur.Catalogue.Core.Web.PageModels;
using SKBKontur.Catalogue.Mutators;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskList
{
    public class TaskListPageModel : PageModelBase<TaskListModelData>
    {
        public TaskListPageModel(PageModelBaseParameters parameters, TaskListModelData taskListModelData)
            : base(parameters)
        {
            Data = taskListModelData;
        }

        public override sealed TaskListModelData Data { get; protected set; }
        public TaskListPaginatorModelData PaginatorModelData { get; set; }
        public SearchPanelHtmlModel HtmlModel { get; set; }

        protected override void Configure(MutatorsConfigurator<TaskListModelData> configurator)
        {
            base.Configure(configurator);
            configurator.Target(data => data.SearchPanel.Ticks.From.Date).Date();
            configurator.Target(data => data.SearchPanel.StartExecutedTicks.From.Date).Date();
            configurator.Target(data => data.SearchPanel.MinimalStartTicks.From.Date).Date();
            configurator.Target(data => data.SearchPanel.Ticks.To.Date).Date();
            configurator.Target(data => data.SearchPanel.StartExecutedTicks.To.Date).Date();
            configurator.Target(data => data.SearchPanel.MinimalStartTicks.To.Date).Date();
        }
    }
}