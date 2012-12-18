using System.Web.Mvc;

using SKBKontur.Catalogue.Core.Web.PageModels;
using SKBKontur.Catalogue.Mutators;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.Html;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Html;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Paging;
using SKBKontur.Catalogue.Core.Web.Models.ModelConfigurations;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models
{
    public class RemoteTaskQueueModel : PageModelBase<RemoteTaskQueuePageModel>, IPaginatorModelData
    {
        public RemoteTaskQueueModel(PageModelBaseParameters parameters, RemoteTaskQueuePageModel remoteTaskQueuePageModel)
            : base(parameters)
        {
            Data = remoteTaskQueuePageModel;
        }

        public string GetUrl(UrlHelper url, int page)
        {
// ReSharper disable Asp.NotResolved
            return url.Action("Run", "RemoteTaskQueue", new {pageNumber = page, searchRequestId = SearchRequestId});
// ReSharper restore Asp.NotResolved
        }

        protected override void Configure(MutatorsConfigurator<RemoteTaskQueuePageModel> configurator)
        {
            base.Configure(configurator);
            configurator.Target(data => data.SearchPanel.Ticks.From.Date).Date();
            configurator.Target(data => data.SearchPanel.Ticks.From.Time).Time();
            configurator.Target(data => data.SearchPanel.StartExecutedTicks.From.Date).Date();
            configurator.Target(data => data.SearchPanel.StartExecutedTicks.From.Time).Time();
            configurator.Target(data => data.SearchPanel.MinimalStartTicks.From.Date).Date();
            configurator.Target(data => data.SearchPanel.MinimalStartTicks.From.Time).Time();
            configurator.Target(data => data.SearchPanel.Ticks.To.Date).Date();
            configurator.Target(data => data.SearchPanel.Ticks.To.Time).Time();
            configurator.Target(data => data.SearchPanel.StartExecutedTicks.To.Date).Date();
            configurator.Target(data => data.SearchPanel.StartExecutedTicks.To.Time).Time();
            configurator.Target(data => data.SearchPanel.MinimalStartTicks.To.Date).Date();
            configurator.Target(data => data.SearchPanel.MinimalStartTicks.To.Time).Time();
        }

        public int TotalPagesCount { get; set; }
        public int PageNumber { get; set; }
        public int PagesWindowSize { get; set; }
        public override sealed RemoteTaskQueuePageModel Data { get; protected set; }
        public string SearchRequestId { get; set; }

        public SearchPanelHtmlModel HtmlModel { get; set; }
    }
}