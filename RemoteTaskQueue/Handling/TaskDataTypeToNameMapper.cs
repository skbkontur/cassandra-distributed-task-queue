using System;
using System.Collections.Generic;

using RemoteQueue.Exceptions;
using RemoteQueue.UserClasses;

namespace RemoteQueue.Handling
{
    public class TaskDataTypeToNameMapper : ITaskDataTypeToNameMapper
    {
        public TaskDataTypeToNameMapper(TaskDataRegistryBase taskDataRegistry)
        {
            foreach(var taskDataInfo in taskDataRegistry.GetAllTaskDataInfos())
            {
                Type type = taskDataInfo.Key;
                string taskName = taskDataInfo.Value;
                typeToName.Add(type, taskName);
                if(nameToType.ContainsKey(taskName))
                    throw new TaskNameDuplicateException(taskName);
                nameToType.Add(taskName, type);
            }
        }

        public string GetTaskName(Type type)
        {
            if(typeToName.ContainsKey(type))
                return typeToName[type];
            throw new TaskDataNotFoundException(type);
        }

        public Type GetTaskType(string name)
        {
            if(nameToType.ContainsKey(name))
                return nameToType[name];
            throw new TaskDataNotFoundException(name);
        }

        private readonly Dictionary<Type, string> typeToName = new Dictionary<Type, string>();
        private readonly Dictionary<string, Type> nameToType = new Dictionary<string, Type>();
    }
}