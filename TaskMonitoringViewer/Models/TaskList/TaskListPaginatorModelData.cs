using System.Web.Mvc;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Paging;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskList
{
    public class TaskListPaginatorModelData : IPaginatorModelData
    {
        public string GetUrl(UrlHelper url, int page)
        {
// ReSharper disable Asp.NotResolved
            return url.Action("Run", "RemoteTaskQueue", new { pageNumber = page, searchRequestId = SearchRequestId });
// ReSharper restore Asp.NotResolved
        }

        public int TotalPagesCount { get; set; }
        public int PageNumber { get; set; }
        public int PagesWindowSize { get; set; }
        public string SearchRequestId { get; set; }
    }
}