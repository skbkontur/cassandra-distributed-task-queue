using RemoteQueue.Handling;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ValueHandlerRegistry
{
    public interface IValueHandlerRegistryCollection
    {
        IValueHandlerRegistry<T> Get<T>() where T : ITaskData;
    }
}