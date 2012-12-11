using System;

using RemoteQueue.Handling;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ValueHandlerRegistry
{
    public interface IValueHandlerRegistry<T>
        where T : ITaskData
    {
        Func<object, object> GetValueHanlder(string path);
    }
}