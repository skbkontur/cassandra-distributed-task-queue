using System;
using System.Collections.Generic;

using ExchangeService.TaskHandlers;

using RemoteQueue.Exceptions;
using RemoteQueue.Handling;
using RemoteQueue.UserClasses;

namespace ExchangeService.UserClasses
{
    public class TaskHandlerRegistry : ITaskHandlerRegistry
    {
        public TaskHandlerRegistry(Func<FakeFailTaskHandler> createFakeFailTaskHandler, Func<FakePeriodicTaskHandler> createFakePeriodicTaskHandler)
        {
            Register(createFakeFailTaskHandler);
            Register(createFakePeriodicTaskHandler);
        }

        public KeyValuePair<Type, Func<ITaskHandler>>[] GetAllTaskHandlerCreators()
        {
            return list.ToArray();
        }

        private void Register<THandler>(Func<THandler> createTaskHandler)
            where THandler : ITaskHandler
        {
            var type = typeof(THandler);
            if(!IsTypedTaskHandler(type)) throw new NotTypedTaskHandlerException(type);
            list.Add(new KeyValuePair<Type, Func<ITaskHandler>>(type, () => (ITaskHandler)createTaskHandler()));
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