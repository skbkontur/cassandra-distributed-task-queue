using System.Web.Mvc;

using SKBKontur.Catalogue.Core.Web.PageModels;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.Html;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Html;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Paging;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models
{
    public class RemoteTaskQueueModel : PageModelBase<RemoteTaskQueueModelData>, IPaginatorModelData
    {
        public RemoteTaskQueueModel(PageModelBaseParameters parameters)
            : base(parameters)
        {
        }

        public string GetUrl(UrlHelper url, int page)
        {
// ReSharper disable Asp.NotResolved
            return url.Action("Run", "RemoteTaskQueue", new {pageNumber = page, searchRequestId = SearchRequestId});
// ReSharper restore Asp.NotResolved
        }

        public int TotalPagesCount { get; set; }
        public int PageNumber { get; set; }
        public int PagesWindowSize { get; set; }
        public override sealed RemoteTaskQueueModelData Data { get; protected set; }
        public string SearchRequestId { get; set; }

        public TaskMetaInfoModel[] TaskModels { get; set; }
        public SearchPanelHtmlModel HtmlModel { get; set; }
    }
}