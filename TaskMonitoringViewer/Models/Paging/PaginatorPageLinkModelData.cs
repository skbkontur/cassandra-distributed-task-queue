using System.Web.Mvc;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Paging
{
    public class PaginatorPageLinkModelData
    {
        public PaginatorPageLinkModelData(IPaginatorModelData paginatorModelData, int pageNumber)
        {
            this.paginatorModelData = paginatorModelData;
            PageNumber = pageNumber;
        }

        public string GetUrl(UrlHelper url)
        {
            return paginatorModelData.GetUrl(url, PageNumber);
        }

        public int PageNumber { get; private set; }
        private readonly IPaginatorModelData paginatorModelData;
    }
}