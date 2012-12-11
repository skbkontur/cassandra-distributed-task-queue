using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders
{
    public interface ITaskDataModelBuilder
    {
        ITaskDataValue Build(string taskId, ITaskData taskData);
    }
}