using SKBKontur.Catalogue.Core.Web.PageModels;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskList
{
    public class TaskListModelData : ModelData
    {
        public SearchPanelModelData SearchPanel { get; set; }
        public int TaskCount { get; set; }
        public TaskMetaInfoModel[]  TaskModels { get; set; }
    }
}