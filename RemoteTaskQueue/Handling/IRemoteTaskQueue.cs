using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;

namespace RemoteQueue.Handling
{
    [PublicAPI]
    public interface IRemoteTaskQueue
    {
        [NotNull]
        EventLogRepository EventLogRepository { get; }

        TaskManipulationResult TryCancelTask([NotNull] string taskId);

        TaskManipulationResult TryRerunTask([NotNull] string taskId, TimeSpan delay);

        [CanBeNull]
        RemoteTaskInfo TryGetTaskInfo([NotNull] string taskId);

        [NotNull]
        RemoteTaskInfo<T> GetTaskInfo<T>([NotNull] string taskId) where T : ITaskData;

        [NotNull, ItemNotNull]
        RemoteTaskInfo[] GetTaskInfos([NotNull, ItemNotNull] string[] taskIds);

        [NotNull, ItemNotNull]
        RemoteTaskInfo<T>[] GetTaskInfos<T>([NotNull, ItemNotNull] string[] taskIds) where T : ITaskData;

        [NotNull]
        Dictionary<string, TaskMetaInformation> GetTaskMetas([NotNull, ItemNotNull] string[] taskIds);

        [NotNull]
        IRemoteTask CreateTask<T>([NotNull] T taskData, [CanBeNull] CreateTaskOptions createTaskOptions = null) where T : ITaskData;

        [NotNull, ItemNotNull]
        string[] GetChildrenTaskIds([NotNull] string taskId);
    }
}