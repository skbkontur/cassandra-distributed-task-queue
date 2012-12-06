using System;

namespace RemoteQueue.Handling
{
    public interface ITaskDataTypeToNameMapper
    {
        string GetTaskName(Type type);
        Type GetTaskType(string name);
    }
}