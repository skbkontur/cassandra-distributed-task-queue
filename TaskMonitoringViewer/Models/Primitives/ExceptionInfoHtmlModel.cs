using SKBKontur.Catalogue.Core.Web.Models.HtmlModels;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Primitives
{
    public class ExceptionInfoHtmlModel : IHtmlModel
    {
        public string Id { get; set; }
        public string ExceptionMessageInfo { get; set; }
    }
}