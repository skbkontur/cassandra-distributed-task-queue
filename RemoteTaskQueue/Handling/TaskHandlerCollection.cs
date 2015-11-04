using System;
using System.Collections.Generic;
using System.Linq;

using RemoteQueue.Configuration;

using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.Handling
{
    public class TaskHandlerCollection : ITaskHandlerCollection
    {
        public TaskHandlerCollection(ITaskDataRegistry taskDataRegistry, ITaskHandlerRegistry taskHandlerRegistry)
        {
            var taskNameToTaskHandler = new Dictionary<string, KeyValuePair<Type, Func<ITaskHandler>>>();
            foreach(var creator in taskHandlerRegistry.GetAllTaskHandlerCreators())
            {
                var taskDataType = GetTaskDataType(creator.Key);
                var taskName = taskDataRegistry.GetTaskName(taskDataType);
                if(taskNameToTaskHandler.ContainsKey(taskName))
                    throw new InvalidProgramStateException(string.Format("There are at least two handlers for task '{0}': '{1}' and '{2}'", taskName, taskNameToTaskHandler[taskName].Key, creator.Key));
                taskNameToTaskHandler.Add(taskName, creator);
            }
            taskNameToTaskHandlerCreator = taskNameToTaskHandler.ToDictionary(x => x.Key, x => x.Value.Value);
        }

        public ITaskHandler CreateHandler(string taskName)
        {
            if(!ContainsHandlerFor(taskName))
                throw new InvalidProgramStateException(string.Format("Handler not found for taskName: {0}", taskName));
            return taskNameToTaskHandlerCreator[taskName]();
        }

        public bool ContainsHandlerFor(string taskName)
        {
            return taskNameToTaskHandlerCreator.ContainsKey(taskName);
        }

        private Type GetTaskDataType(Type handlerType)
        {
            if(handlerType.IsGenericType && handlerType.GetGenericTypeDefinition() == typeof(TaskHandler<>))
                return handlerType.GetGenericArguments()[0];
            return GetTaskDataType(handlerType.BaseType);
        }

        private readonly Dictionary<string, Func<ITaskHandler>> taskNameToTaskHandlerCreator;
    }
}