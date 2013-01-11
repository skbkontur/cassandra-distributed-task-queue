using System;

namespace RemoteQueue.Handling
{
    public interface IRemoteTaskQueue
    {
        bool CancelTask(string taskId);
        bool RerunTask(string taskId, TimeSpan delay);

        RemoteTaskInfo GetTaskInfo(string taskId);
        RemoteTaskInfo<T> GetTaskInfo<T>(string taskId) where T : ITaskData;

        IRemoteTask CreateTask<T>(T taskData, CreateTaskOptions createTaskOptions = null) where T : ITaskData;
    }
}