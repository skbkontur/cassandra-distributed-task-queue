using System;

using JetBrains.Annotations;

namespace RemoteQueue.Handling
{
    public interface IRemoteTaskQueue
    {
        TaskManipulationResult TryCancelTask([NotNull] string taskId);
        TaskManipulationResult TryRerunTask([NotNull] string taskId, TimeSpan delay);

        [NotNull]
        RemoteTaskInfo GetTaskInfo([NotNull] string taskId);

        [NotNull]
        RemoteTaskInfo<T> GetTaskInfo<T>([NotNull] string taskId) where T : ITaskData;

        [NotNull]
        RemoteTaskInfo[] GetTaskInfos([NotNull] string[] taskIds);

        [NotNull]
        RemoteTaskInfo<T>[] GetTaskInfos<T>([NotNull] string[] taskIds) where T : ITaskData;

        [NotNull]
        IRemoteTask CreateTask<T>([NotNull] T taskData, [CanBeNull] CreateTaskOptions createTaskOptions = null) where T : ITaskData;

        [NotNull]
        string[] GetChildrenTaskIds([NotNull] string taskId);
    }
}