using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders
{
    public interface ITaskViewModelBuilder
    {
        TaskViewModel Build(RemoteTaskInfo remoteTaskInfo);
    }
}