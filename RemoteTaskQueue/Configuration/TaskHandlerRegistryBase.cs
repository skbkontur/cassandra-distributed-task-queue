using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using RemoteQueue.Handling;

using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.Configuration
{
    public abstract class TaskHandlerRegistryBase : ITaskHandlerRegistry
    {
        [NotNull]
        public KeyValuePair<Type, Func<ITaskHandler>>[] GetAllTaskHandlerCreators()
        {
            return list.ToArray();
        }

        protected void Register(Type handlerType, Func<ITaskHandler> createTaskHandler)
        {
            if(!IsTypedTaskHandler(handlerType))
                throw new InvalidProgramStateException(string.Format("Type '{0}' doesn't implement 'TaskHander<>'", handlerType.FullName));
            list.Add(new KeyValuePair<Type, Func<ITaskHandler>>(handlerType, createTaskHandler));
        }

        protected void Register<THandler>(Func<THandler> createTaskHandler)
            where THandler : ITaskHandler
        {
            Register(typeof(THandler), () => (ITaskHandler)createTaskHandler());
        }

        private static bool IsTypedTaskHandler(Type handlerType)
        {
            if(handlerType == null)
                return false;
            if(handlerType.IsGenericType && handlerType.GetGenericTypeDefinition() == typeof(TaskHandler<>))
                return true;
            return IsTypedTaskHandler(handlerType.BaseType);
        }

        private readonly List<KeyValuePair<Type, Func<ITaskHandler>>> list = new List<KeyValuePair<Type, Func<ITaskHandler>>>();
    }
}