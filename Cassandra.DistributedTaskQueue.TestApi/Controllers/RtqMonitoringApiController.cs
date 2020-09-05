using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Mvc;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api;

namespace SkbKontur.Cassandra.DistributedTaskQueue.TestApi.Controllers
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
        [Route("available-task-names")]
        public string[] GetAllTaskNames()
        {
            return rtqMonitoringApi.GetAllTasksNames();
        }

        [HttpPost]
        [Route("tasks/search")]
        public RtqMonitoringSearchResults Search([FromBody] RtqMonitoringSearchRequest searchRequest, int from, int size)
        {
            return rtqMonitoringApi.Search(searchRequest, from, size);
        }

        [HttpGet]
        [Route("tasks/{taskId}")]
        public RtqMonitoringTaskModel GetTaskDetails([NotNull] string taskId)
        {
            return rtqMonitoringApi.GetTaskDetails(taskId);
        }

        [HttpPost]
        [Route("tasks/cancel")]
        public Dictionary<string, TaskManipulationResult> CancelTasks([FromBody] string[] ids)
        {
            return rtqMonitoringApi.CancelTasks(ids);
        }

        [HttpPost]
        [Route("tasks/rerun")]
        public Dictionary<string, TaskManipulationResult> RerunTasks([FromBody] string[] ids)
        {
            return rtqMonitoringApi.RerunTasks(ids);
        }

        [HttpPost]
        [Route("tasks/rerun-by-request")]
        public Dictionary<string, TaskManipulationResult> RerunTasksBySearchQuery([FromBody] RtqMonitoringSearchRequest searchRequest)
        {
            return rtqMonitoringApi.RerunTasksBySearchQuery(searchRequest);
        }

        [HttpPost]
        [Route("tasks/cancel-by-request")]
        public Dictionary<string, TaskManipulationResult> CancelTasksBySearchQuery([FromBody] RtqMonitoringSearchRequest searchRequest)
        {
            return rtqMonitoringApi.CancelTasksBySearchQuery(searchRequest);
        }

        private readonly IRtqMonitoringApi rtqMonitoringApi;
    }
}