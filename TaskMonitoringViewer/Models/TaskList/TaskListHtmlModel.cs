using SKBKontur.Catalogue.Core.Web.Models.HtmlModels;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskList.SearchPanel;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskList
{
    public class TaskListHtmlModel
    {
        public TextHtmlModel TaskCount { get; set; }
        public SearchPanelHtmlModel SearchPanel { get; set; }
        public TaskMetaInfoHtmlModel[] Tasks { get; set; }
        public CounterHtmlModel Counter { get; set; }
    }
}