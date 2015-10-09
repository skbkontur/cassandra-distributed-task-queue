using System;

using JetBrains.Annotations;

namespace RemoteQueue.Handling
{
    public interface ITaskDataTypeToNameMapper
    {
        [NotNull]
        string GetTaskName([NotNull] Type type);

        [NotNull]
        Type GetTaskType([NotNull] string name);

        bool TryGetTaskType([NotNull] string name, out Type taskType);

        bool TryGetTaskName([NotNull] Type type, out string name);
    }
}