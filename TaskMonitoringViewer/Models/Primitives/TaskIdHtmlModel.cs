using System.Web.Mvc;

using SKBKontur.Catalogue.Core.Web.Models.HtmlModels;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Primitives
{
    public class TaskIdHtmlModel : IHtmlModel
    {
        public TaskIdHtmlModel(int page, string searchRequestId)
        {
            this.page = page;
            this.searchRequestId = searchRequestId;
        }

        public string Id { get; set; }
        public string Value { get; set; }

        public string GetDetailsUrl(UrlHelper url)
        {
            return url.GetTaskDetailsUrl(Value, page, searchRequestId);
        }

        private readonly int page;
        private readonly string searchRequestId;
    }
}