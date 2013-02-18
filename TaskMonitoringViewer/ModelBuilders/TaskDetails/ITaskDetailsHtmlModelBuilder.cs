using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskDetails;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.TaskDetails
{
    public interface ITaskDetailsHtmlModelBuilder
    {
        TaskDetailsHtmlModel Build(TaskDetailsPageModel pageModel);
    }
}