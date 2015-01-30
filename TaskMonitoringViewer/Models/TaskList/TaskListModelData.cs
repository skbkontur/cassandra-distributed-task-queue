using SKBKontur.Catalogue.Core.Web.Models.DateAndTimeModels;
using SKBKontur.Catalogue.Core.Web.PageModels;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskList.SearchPanel;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskList
{
    public class TaskListModelData : ModelData
    {
        public int TaskCount { get; set; }
        public SearchPanelModelData SearchPanel { get; set; }
        public TaskMetaInfoModel[]  TaskModels { get; set; }
        public DateAndTime RestartTime { get; set; }
    }
}