using System.Collections.Generic;

using JetBrains.Annotations;

using RemoteQueue.Handling;

namespace RemoteTaskQueue.Monitoring.Api
{
    public interface IRemoteTaskQueueMonitoringApi
    {
        [NotNull, ItemNotNull]
        string[] GetAllTaksNames();

        [NotNull]
        RemoteTaskQueueSearchResults Search([NotNull] RemoteTaskQueueSearchRequest searchRequest, int from, int size);

        [CanBeNull]
        RemoteTaskInfoModel GetTaskDetails([NotNull] string taskId);

        [NotNull]
        Dictionary<string, TaskManipulationResult> CancelTasks([NotNull, ItemNotNull] string[] ids);

        [NotNull]
        Dictionary<string, TaskManipulationResult> RerunTasks([NotNull, ItemNotNull] string[] ids);

        [NotNull]
        Dictionary<string, TaskManipulationResult> RerunTasksBySearchQuery([NotNull] RemoteTaskQueueSearchRequest searchRequest);

        [NotNull]
        Dictionary<string, TaskManipulationResult> CancelTasksBySearchQuery([NotNull] RemoteTaskQueueSearchRequest searchRequest);

        void ResetTicksHolderInMemoryState();
    }
}