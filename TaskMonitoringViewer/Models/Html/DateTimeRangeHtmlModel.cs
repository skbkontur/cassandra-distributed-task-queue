using SKBKontur.Catalogue.Core.Web.Models.HtmlModels;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Html
{
    public class DateTimeRangeHtmlModel
    {
        public DateAndTimeHtmlModel From { get; set; }
        public DateAndTimeHtmlModel To { get; set; }
    }
}