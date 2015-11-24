using System;
using System.Collections.Generic;

using RemoteQueue.Exceptions;
using RemoteQueue.Handling;

namespace RemoteQueue.UserClasses
{
    public abstract class TaskHandlerRegistryBase
    {
        public KeyValuePair<Type, Func<ITaskHandler>>[] GetAllTaskHandlerCreators()
        {
            return list.ToArray();
        }

        protected void Register(Type handlerType, Func<ITaskHandler> createTaskHandler)
        {
            if (!IsTypedTaskHandler(handlerType)) throw new NotTypedTaskHandlerException(handlerType);
            list.Add(new KeyValuePair<Type, Func<ITaskHandler>>(handlerType, createTaskHandler));
        }

        protected void Register<THandler>(Func<THandler> createTaskHandler)
            where THandler : ITaskHandler
        {
            Register(typeof(THandler), () => (ITaskHandler)createTaskHandler());
        }

        private bool IsTypedTaskHandler(Type handlerType)
        {
            if(handlerType == null) return false;
            if(handlerType.IsGenericType && handlerType.GetGenericTypeDefinition() == typeof(TaskHandler<>))
                return true;
            return IsTypedTaskHandler(handlerType.BaseType);
        }

        private readonly List<KeyValuePair<Type, Func<ITaskHandler>>> list = new List<KeyValuePair<Type, Func<ITaskHandler>>>();
    }
}