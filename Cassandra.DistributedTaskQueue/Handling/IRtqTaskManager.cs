using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
{
    [PublicAPI]
    public interface IRtqTaskManager
    {
        TaskManipulationResult TryCancelTask([NotNull] string taskId);

        TaskManipulationResult TryRerunTask([NotNull] string taskId, TimeSpan delay);

        [CanBeNull]
        RemoteTaskInfo TryGetTaskInfo([NotNull] string taskId);

        [NotNull]
        RemoteTaskInfo<T> GetTaskInfo<T>([NotNull] string taskId)
            where T : IRtqTaskData;

        [NotNull, ItemNotNull]
        RemoteTaskInfo[] GetTaskInfos([NotNull, ItemNotNull] string[] taskIds);

        [NotNull, ItemNotNull]
        RemoteTaskInfo<T>[] GetTaskInfos<T>([NotNull, ItemNotNull] string[] taskIds)
            where T : IRtqTaskData;

        [NotNull]
        Dictionary<string, TaskMetaInformation> GetTaskMetas([NotNull, ItemNotNull] string[] taskIds);

        [NotNull, ItemNotNull]
        string[] GetChildrenTaskIds([NotNull] string taskId);

        [NotNull, ItemNotNull]
        string[] GetRecentTaskIds([CanBeNull] Timestamp fromTimestampExclusive, [NotNull] Timestamp toTimestampInclusive);
    }
}