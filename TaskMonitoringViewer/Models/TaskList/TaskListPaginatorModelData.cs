using System.Web.Mvc;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Paging;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskList
{
    public class TaskListPaginatorModelData : IPaginatorModelData
    {
        public string GetUrl(UrlHelper url, int page)
        {
            return url.Action("Run", "RemoteTaskQueue", new {pageNumber = page, searchRequestId = SearchRequestId});
        }

        public int TotalPagesCount { get; set; }
        public int PageNumber { get; set; }
        public int PagesWindowSize { get; set; }
        public string SearchRequestId { get; set; }
    }
}