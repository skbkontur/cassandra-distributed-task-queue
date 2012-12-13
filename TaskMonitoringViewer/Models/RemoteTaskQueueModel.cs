using System.Web.Mvc;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Paging;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models
{
    public class RemoteTaskQueueModel : IPaginatorModelData
    {
        public string GetUrl(UrlHelper url, int page)
        {
// ReSharper disable Asp.NotResolved
            return url.Action("Run", "RemoteTaskQueue", new { page });
// ReSharper restore Asp.NotResolved
        }

        public int TotalPagesCount { get; set; }
        public int PageNumber { get; set; }
        public int PagesWindowSize { get; set; }

        public TaskMetaInfoModel[] TaskModels { get; set; }
    }
}