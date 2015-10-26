using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.Sharding.Hashes;

namespace RemoteQueue.Handling
{
    public class TaskTopicResolver : ITaskTopicResolver
    {
        public TaskTopicResolver(ITaskDataTypeToNameMapper taskDataTypeToNameMapper)
        {
            foreach(var taskName in taskDataTypeToNameMapper.GetAllTaskNames())
                nameToTopic.Add(taskName, ResolveTopic(taskName));
        }

        [NotNull]
        private static string ResolveTopic([NotNull] string taskName)
        {
            return (Math.Abs(taskName.GetPersistentHashCode()) % topicsCount).ToString(CultureInfo.InvariantCulture);
        }

        [NotNull]
        public string[] GetAllTaskTopics()
        {
            return nameToTopic.Values.Distinct().ToArray();
        }

        [NotNull]
        public string GetTaskTopic([NotNull] string taskName)
        {
            return nameToTopic[taskName];
        }

        private const int topicsCount = 8;
        private readonly Dictionary<string, string> nameToTopic = new Dictionary<string, string>();
    }
}