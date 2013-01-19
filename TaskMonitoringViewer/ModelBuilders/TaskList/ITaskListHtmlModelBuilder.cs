using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskList;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.TaskList
{
    public interface ITaskListHtmlModelBuilder
    {
        TaskListHtmlModel Build(TaskListPageModel pageModel);
    }
}