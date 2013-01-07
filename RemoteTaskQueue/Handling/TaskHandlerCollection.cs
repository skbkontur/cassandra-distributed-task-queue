using System;
using System.Collections.Generic;
using System.Linq;

using RemoteQueue.Exceptions;
using RemoteQueue.UserClasses;

namespace RemoteQueue.Handling
{
    public class TaskHandlerCollection : ITaskHandlerCollection
    {
        public TaskHandlerCollection(ITaskDataTypeToNameMapper typeToNameMapper, TaskHandlerRegistryBase taskHandlerRegistry)
        {
            var taskNameToTaskHandler = new Dictionary<string, KeyValuePair<Type, Func<ITaskHandler>>>();
            foreach(var creator in taskHandlerRegistry.GetAllTaskHandlerCreators())
            {
                var taskDataType = GetTaskDataType(creator.Key);
                var taskName = typeToNameMapper.GetTaskName(taskDataType);
                if(taskNameToTaskHandler.ContainsKey(taskName))
                    throw new TooManyTaskHandlersException(taskName, taskNameToTaskHandler[taskName].Key, creator.Key);
                taskNameToTaskHandler.Add(taskName, creator);
            }
            taskNameToTaskHandlerCreator = taskNameToTaskHandler.ToDictionary(x => x.Key, x => x.Value.Value);
        }

        public ITaskHandler CreateHandler(string taskName)
        {
            if(!taskNameToTaskHandlerCreator.ContainsKey(taskName))
                throw new TaskHandlerNotFoundException(taskName);
            return taskNameToTaskHandlerCreator[taskName]();
        }

        private Type GetTaskDataType(Type handlerType)
        {
            if(handlerType.IsGenericType && handlerType.GetGenericTypeDefinition() == typeof(TaskHandler<>))
                return handlerType.GetGenericArguments()[0];
            return GetTaskDataType(handlerType.BaseType);
        }

        private readonly Dictionary<string, Func<ITaskHandler>> taskNameToTaskHandlerCreator = new Dictionary<string, Func<ITaskHandler>>();
    }
}