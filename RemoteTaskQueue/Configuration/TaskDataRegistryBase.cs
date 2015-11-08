using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using JetBrains.Annotations;

using RemoteQueue.Handling;

using SKBKontur.Catalogue.Core.Sharding.Hashes;
using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.Configuration
{
    public abstract class TaskDataRegistryBase : ITaskDataRegistry
    {
        protected void Register<T>() where T : ITaskData
        {
            var taskType = typeof(T);
            var taskName = taskType.GetTaskName();
            if(nameToType.ContainsKey(taskName))
                throw new InvalidProgramStateException(string.Format("Duplicate taskName: {0}", taskName));
            typeToName.Add(taskType, taskName);
            nameToType.Add(taskName, taskType);
            nameToTopic.Add(taskName, ResolveTopic(taskType, taskName));
        }

        [NotNull]
        private static string ResolveTopic([NotNull] Type taskType, [NotNull] string taskName)
        {
            var taskTopic = taskType.TryGetTaskTopic();
            if(!string.IsNullOrWhiteSpace(taskTopic))
                return taskTopic;
            return (Math.Abs(taskName.GetPersistentHashCode()) % topicsCount).ToString(CultureInfo.InvariantCulture);
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
            Type taskType;
            if(!nameToType.TryGetValue(taskName, out taskType))
                throw new InvalidProgramStateException(string.Format("TaskData with name '{0}' not registered", taskName));
            return taskType;
        }

        public bool TryGetTaskType([NotNull] string taskName, out Type taskType)
        {
            return nameToType.TryGetValue(taskName, out taskType);
        }

        [NotNull]
        public string[] GetAllTaskTopics()
        {
            return nameToTopic.Values.Distinct().ToArray();
        }

        [NotNull]
        public string GetTaskTopic([NotNull] string taskName)
        {
            string taskTopic;
            if(!nameToTopic.TryGetValue(taskName, out taskTopic))
                throw new InvalidProgramStateException(string.Format("TaskData with name '{0}' not registered", taskName));
            return taskTopic;
        }

        private const int topicsCount = 2;
        private readonly Dictionary<Type, string> typeToName = new Dictionary<Type, string>();
        private readonly Dictionary<string, Type> nameToType = new Dictionary<string, Type>();
        private readonly Dictionary<string, string> nameToTopic = new Dictionary<string, string>();
    }
}