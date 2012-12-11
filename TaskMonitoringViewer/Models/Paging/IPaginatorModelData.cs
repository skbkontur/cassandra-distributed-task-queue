using System.Web.Mvc;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Paging
{
    public interface IPaginatorModelData
    {
        string GetUrl(UrlHelper url, int page);
        int TotalPagesCount { get; }
        int PageNumber { get; }
        int PagesWindowSize { get; }
    }
}