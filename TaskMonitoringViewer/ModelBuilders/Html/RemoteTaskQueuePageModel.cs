using SKBKontur.Catalogue.Core.Web.PageModels;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.Html
{
    public class RemoteTaskQueuePageModel : ModelData
    {
        public SearchPanelModelData SearchPanel { get; set; }
        public int TaskCount { get; set; }
        public TaskMetaInfoModel[]  TaskModels { get; set; }
    }
}