using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskDetails.TaskData;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.TaskDetails
{
    public interface ITaskDataModelBuilder
    {
        ITaskDataValue Build(string taskId, ITaskData taskData);
    }
}