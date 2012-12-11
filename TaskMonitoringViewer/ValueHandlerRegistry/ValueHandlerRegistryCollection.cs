using System;
using System.Collections.Concurrent;

using GroboContainer.Core;
using GroboContainer.Impl.Exceptions;

using RemoteQueue.Handling;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ValueHandlerRegistry
{
    public class ValueHandlerRegistryCollection : IValueHandlerRegistryCollection
    {
        public ValueHandlerRegistryCollection(IContainer container)
        {
            this.container = container;
        }

        public IValueHandlerRegistry<T> Get<T>() where T : ITaskData
        {
            return (IValueHandlerRegistry<T>)valueHandlerRegistryByType.GetOrAdd(typeof(T), GetInternal<T>());
        }

        private IValueHandlerRegistry<T> GetInternal<T>() where T : ITaskData
        {
            try
            {
                var abstractionType = typeof(IValueHandlerRegistry<>).MakeGenericType(typeof(T));
                return (IValueHandlerRegistry<T>)container.Get(abstractionType);
            }
            catch(ContainerException)
            {
                return new EmptyValueHandlerRegistry<T>();
            }
        }

        private readonly IContainer container;
        private readonly ConcurrentDictionary<Type, object> valueHandlerRegistryByType = new ConcurrentDictionary<Type, object>();
    }
}