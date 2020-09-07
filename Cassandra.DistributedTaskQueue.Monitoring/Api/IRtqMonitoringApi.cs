using System.Collections.Generic;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api
{
    [PublicAPI]
    public interface IRtqMonitoringApi
    {
        [NotNull, ItemNotNull]
        string[] GetAllTasksNames();

        [NotNull]
        RtqMonitoringSearchResults Search([NotNull] RtqMonitoringSearchRequest searchRequest);

        [CanBeNull]
        RtqMonitoringTaskModel GetTaskDetails([NotNull] string taskId);

        [NotNull]
        Dictionary<string, TaskManipulationResult> CancelTasks([NotNull, ItemNotNull] string[] ids);

        [NotNull]
        Dictionary<string, TaskManipulationResult> RerunTasks([NotNull, ItemNotNull] string[] ids);

        [NotNull]
        Dictionary<string, TaskManipulationResult> RerunTasksBySearchQuery([NotNull] RtqMonitoringSearchRequest searchRequest);

        [NotNull]
        Dictionary<string, TaskManipulationResult> CancelTasksBySearchQuery([NotNull] RtqMonitoringSearchRequest searchRequest);
    }
}