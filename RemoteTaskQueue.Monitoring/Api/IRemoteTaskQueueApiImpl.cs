using System.Collections.Generic;

using JetBrains.Annotations;

using RemoteQueue.Handling;

namespace RemoteTaskQueue.Monitoring.Api
{
    public interface IRemoteTaskQueueApiImpl
    {
        string[] GetAllTaksNames();

        RemoteTaskQueueSearchResults Search(RemoteTaskQueueSearchRequest searchRequest);

        [NotNull]
        RemoteTaskInfoModel GetTaskDetails(string taskId);

        [NotNull]
        Dictionary<string, TaskManipulationResult> CancelTasks(string[] ids);

        [NotNull]
        Dictionary<string, TaskManipulationResult> RerunTasks(string[] ids);

        [NotNull]
        Dictionary<string, TaskManipulationResult> RerunTasksBySearchQuery(RemoteTaskQueueSearchRequest searchRequest);

        [NotNull]
        Dictionary<string, TaskManipulationResult> CancelTasksBySearchQuery(RemoteTaskQueueSearchRequest searchRequest);

    }
}