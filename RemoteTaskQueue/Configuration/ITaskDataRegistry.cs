using System;

using JetBrains.Annotations;

namespace RemoteQueue.Configuration
{
    public interface ITaskDataRegistry
    {
        [NotNull]
        string[] GetAllTaskNames();

        [NotNull]
        string GetTaskName([NotNull] Type type);

        [NotNull]
        Type GetTaskType([NotNull] string taskName);

        bool TryGetTaskType([NotNull] string taskName, out Type taskType);

        [NotNull]
        string[] GetAllTaskTopics();

        [NotNull]
        string GetTaskTopic([NotNull] string taskName);
    }
}