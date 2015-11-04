using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using RemoteQueue.Handling;

using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.Configuration
{
    public abstract class TaskDataRegistryBase : ITaskDataRegistry
    {
        protected void Register<T>([NotNull] string taskName) where T : ITaskData
        {
            var taskType = typeof(T);
            typeToName.Add(taskType, taskName);
            if(nameToType.ContainsKey(taskName))
                throw new InvalidProgramStateException(string.Format("Duplicate taskName: {0}", taskName));
            nameToType.Add(taskName, taskType);
        }

        [NotNull]
        public string[] GetAllTaskNames()
        {
            return nameToType.Keys.ToArray();
        }

        [NotNull]
        public string GetTaskName([NotNull] Type type)
        {
            string taskName;
            if(!typeToName.TryGetValue(type, out taskName))
                throw new InvalidProgramStateException(string.Format("TaskData with type '{0}' not registered", type.FullName));
            return taskName;
        }

        [NotNull]
        public Type GetTaskType([NotNull] string taskName)
        {
            Type type;
            if(!TryGetTaskType(taskName, out type))
                throw new InvalidProgramStateException(string.Format("TaskData with name '{0}' not registered", taskName));
            return type;
        }

        public bool TryGetTaskType([NotNull] string taskName, out Type taskType)
        {
            return nameToType.TryGetValue(taskName, out taskType);
        }

        private readonly Dictionary<Type, string> typeToName = new Dictionary<Type, string>();
        private readonly Dictionary<string, Type> nameToType = new Dictionary<string, Type>();
    }
}