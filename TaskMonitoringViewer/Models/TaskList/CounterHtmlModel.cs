using SKBKontur.Catalogue.Core.Web.Blocks.ActionButton;
using SKBKontur.Catalogue.Core.Web.Models.HtmlModels;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskList
{
    public class CounterHtmlModel
    {
        public ActionButtonHtmlModel OpenCounter { get; set; }
        public ActionButtonHtmlModel RestartCounter { get; set; }
        public DateAndTimeHtmlModel RestartDate { get; set; }
    }
}