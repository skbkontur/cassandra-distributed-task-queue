using System;

using GroboContainer.Infection;

using RemoteQueue.Handling;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ValueHandlerRegistry
{
    [IgnoredImplementation]
    public class EmptyValueHandlerRegistry<T> : IValueHandlerRegistry<T>
        where T : ITaskData
    {
        public Func<object, object> GetValueHanlder(string path)
        {
            return null;
        }
    }
}