using System;

namespace RemoteQueue.Handling
{
    public interface ITaskDataTypeToNameMapper
    {
        string GetTaskName(Type type);
        Type GetTaskType(string name);
        bool TryGetTaskType(string name, out Type taskType);
        bool TryGetTaskName(Type type, out string name);
    }
}