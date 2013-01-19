using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskDetails;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.TaskDetails
{
    public interface ITaskDetailsModelBuilder
    {
        TaskDetailsModel Build(RemoteTaskInfo remoteTaskInfo, int? pageNumber, string searchRequestId);
    }
}