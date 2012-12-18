using System;

namespace RemoteQueue.Handling
{
    public interface IRemoteTaskQueue
    {
        bool CancelTask(string taskId);
        bool RerunTask(string taskId, TimeSpan delay);

        RemoteTaskInfo GetTaskInfo(string taskId);
        RemoteTaskInfo<T> GetTaskInfo<T>(string taskId) where T : ITaskData;

        IRemoteTask CreateTask<T>(T taskData) where T : ITaskData;
        IRemoteTask CreateTask<T>(T taskData, string parentTaskId) where T : ITaskData;

        string Queue<T>(T taskData) where T : ITaskData;
        string Queue<T>(T taskData, string parentTaskId) where T : ITaskData;
        string Queue<T>(T taskData, TimeSpan delay) where T : ITaskData;
        string Queue<T>(T taskData, TimeSpan delay, string parentTaskId) where T : ITaskData;
    }
}