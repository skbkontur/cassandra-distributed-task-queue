using SKBKontur.Catalogue.Core.Web.Models.HtmlModels;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Primitives
{
    public class TaskDateTimeHtmlModel : IHtmlModel
    {
        public string Id { get; set; }
        public string DateTime { get; set; }
        public long? Ticks { get; set; }
        public bool HideTicks { get; set; }
    }
}