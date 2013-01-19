using SKBKontur.Catalogue.Core.Web.Models.DateAndTimeModels;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskList
{
    public class DateTimeRangeModel
    {
        public DateAndTime From { get; set; }
        public DateAndTime To { get; set; }
    }
}