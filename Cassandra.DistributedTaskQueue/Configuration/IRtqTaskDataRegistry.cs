using System;

using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Configuration
{
    public interface IRtqTaskDataRegistry
    {
        [NotNull, ItemNotNull]
        string[] GetAllTaskNames();

        [NotNull]
        string GetTaskName([NotNull] Type type);

        [NotNull]
        Type GetTaskType([NotNull] string taskName);

        bool TryGetTaskType([NotNull] string taskName, out Type taskType);

        [NotNull, ItemNotNull]
        string[] GetAllTaskTopics();

        [NotNull]
        string GetTaskTopic([NotNull] string taskName);
    }
}