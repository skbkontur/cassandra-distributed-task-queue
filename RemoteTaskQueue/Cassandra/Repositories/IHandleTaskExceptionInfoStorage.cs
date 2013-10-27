using System;

using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Cassandra.Repositories
{
    public interface IHandleTaskExceptionInfoStorage
    {
        void TryAddExceptionInfo(string taskId, Exception e);
        bool TryGetExceptionInfo(string taskId, out TaskExceptionInfo exceptionInfo);
        TaskExceptionInfo[] ReadExceptionInfosQuiet(string[] taskIds);
    }
}