using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

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
                var type = taskDataInfo.Key;
                var taskName = taskDataInfo.Value;
                typeToName.Add(type, taskName);
                if(nameToType.ContainsKey(taskName))
                    throw new TaskNameDuplicateException(taskName);
                nameToType.Add(taskName, type);
            }
        }

        [NotNull]
        public string[] GetAllTaskNames()
        {
            return nameToType.Keys.ToArray();
        }

        [NotNull]
        public string GetTaskName([NotNull] Type type)
        {
            string name;
            if(!TryGetTaskName(type, out name))
                throw new TaskDataNotFoundException(type);
            return name;
        }

        [NotNull]
        public Type GetTaskType([NotNull] string name)
        {
            Type type;
            if(!TryGetTaskType(name, out type))
                throw new TaskDataNotFoundException(name);
            return type;
        }

        public bool TryGetTaskType([NotNull] string name, out Type taskType)
        {
            return nameToType.TryGetValue(name, out taskType);
        }

        public bool TryGetTaskName([NotNull] Type type, out string name)
        {
            return typeToName.TryGetValue(type, out name);
        }

        private readonly Dictionary<Type, string> typeToName = new Dictionary<Type, string>();
        private readonly Dictionary<string, Type> nameToType = new Dictionary<string, Type>();
    }
}