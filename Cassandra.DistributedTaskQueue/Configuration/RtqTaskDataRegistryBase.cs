using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using SkbKontur.Cassandra.DistributedTaskQueue.Commons;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

#nullable enable

namespace SkbKontur.Cassandra.DistributedTaskQueue.Configuration
{
    public abstract class RtqTaskDataRegistryBase : IRtqTaskDataRegistry
    {
        protected RtqTaskDataRegistryBase(bool allTasksShouldHaveTopic = false)
        {
            this.allTasksShouldHaveTopic = allTasksShouldHaveTopic;
        }

        protected void Register<T>() where T : IRtqTaskData
        {
            var taskType = typeof(T);
            var taskName = taskType.GetTaskName();
            if (nameToType.ContainsKey(taskName))
                throw new InvalidOperationException($"Duplicate taskName: {taskName}");
            typeToName.Add(taskType, taskName);
            nameToType.Add(taskName, taskType);
            nameToTopic.Add(taskName, ResolveTopic(taskType, taskName, allTasksShouldHaveTopic));
        }

        private static string ResolveTopic(Type taskType, string taskName, bool taskTopicIsRequired)
        {
            var taskTopic = taskType.TryGetTaskTopic(taskTopicIsRequired);
            if (!string.IsNullOrWhiteSpace(taskTopic))
                return taskTopic!;
            return ShardingHelpers.GetShard(taskName.GetPersistentHashCode(), topicsCount).ToString(CultureInfo.InvariantCulture);
        }

        public string[] GetAllTaskNames()
        {
            return nameToType.Keys.ToArray();
        }

        public string GetTaskName(Type type)
        {
            if (!typeToName.TryGetValue(type, out var taskName))
                throw new InvalidOperationException($"TaskData with type '{type.FullName}' not registered");
            return taskName;
        }

        public Type GetTaskType(string taskName)
        {
            if (!nameToType.TryGetValue(taskName, out var taskType))
                throw new InvalidOperationException($"TaskData with name '{taskName}' not registered");
            return taskType;
        }

        public bool TryGetTaskType(string taskName, out Type? taskType)
        {
            return nameToType.TryGetValue(taskName, out taskType);
        }

        public string[] GetAllTaskTopics()
        {
            return nameToTopic.Values.Distinct().ToArray();
        }

        public (string TaskName, string TopicName)[] GetAllTaskNamesWithTopics()
        {
            return nameToTopic.Select(kvp => (kvp.Key, kvp.Value)).OrderBy(x => x).ToArray();
        }

        public string GetTaskTopic(string taskName)
        {
            if (!nameToTopic.TryGetValue(taskName, out var taskTopic))
                throw new InvalidOperationException($"TaskData with name '{taskName}' not registered");
            return taskTopic;
        }

        private const int topicsCount = 2;
        private readonly Dictionary<Type, string> typeToName = new Dictionary<Type, string>();
        private readonly Dictionary<string, Type> nameToType = new Dictionary<string, Type>();
        private readonly Dictionary<string, string> nameToTopic = new Dictionary<string, string>();
        private readonly bool allTasksShouldHaveTopic;
    }
}