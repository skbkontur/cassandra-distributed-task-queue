using System.Collections.Generic;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Mvc;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TestService
{
    [ApiController]
    [Route("remote-task-queue")]
    public class RtqMonitoringApiController : ControllerBase
    {
        public RtqMonitoringApiController(IRtqMonitoringApi rtqMonitoringApi)
        {
            this.rtqMonitoringApi = rtqMonitoringApi;
        }

        [HttpGet]
        [NotNull, ItemNotNull]
        [Route("available-task-names")]
        public string[] GetAllTaskNames()
        {
            return rtqMonitoringApi.GetAllTasksNames();
        }

        [NotNull]
        [HttpPost]
        [Route("tasks/search")]
        public RtqMonitoringSearchResults Search([NotNull] [FromBody] RtqMonitoringSearchRequest searchRequest)
        {
            return rtqMonitoringApi.Search(searchRequest);
        }

        [NotNull]
        [HttpGet]
        [Route("tasks/{taskId}")]
        public RtqMonitoringTaskModel GetTaskDetails([NotNull] string taskId)
        {
            return rtqMonitoringApi.GetTaskDetails(taskId);
        }

        [NotNull]
        [HttpPost]
        [Route("tasks/cancel")]
        public Dictionary<string, TaskManipulationResult> CancelTasks([NotNull, ItemNotNull] [FromBody] string[] ids)
        {
            return rtqMonitoringApi.CancelTasks(ids);
        }

        [NotNull]
        [HttpPost]
        [Route("tasks/rerun")]
        public Dictionary<string, TaskManipulationResult> RerunTasks([NotNull, ItemNotNull] [FromBody] string[] ids)
        {
            return rtqMonitoringApi.RerunTasks(ids);
        }

        [NotNull]
        [HttpPost]
        [Route("tasks/rerun-by-request")]
        public Dictionary<string, TaskManipulationResult> RerunTasksBySearchQuery([NotNull] [FromBody] RtqMonitoringSearchRequest searchRequest)
        {
            return rtqMonitoringApi.RerunTasksBySearchQuery(searchRequest);
        }

        [NotNull]
        [HttpPost]
        [Route("tasks/cancel-by-request")]
        public Dictionary<string, TaskManipulationResult> CancelTasksBySearchQuery([NotNull] [FromBody] RtqMonitoringSearchRequest searchRequest)
        {
            return rtqMonitoringApi.CancelTasksBySearchQuery(searchRequest);
        }

        private readonly IRtqMonitoringApi rtqMonitoringApi;
    }
}