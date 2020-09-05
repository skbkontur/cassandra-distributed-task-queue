using System.Collections.Generic;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api;

namespace SkbKontur.Cassandra.DistributedTaskQueue.TestApi.Controllers
{
    public class RtqMonitoringApiStub : IRtqMonitoringApi
    {
        public string[] GetAllTasksNames()
        {
            throw new System.NotImplementedException();
        }

        public RtqMonitoringSearchResults Search(RtqMonitoringSearchRequest searchRequest, int @from, int size)
        {
            throw new System.NotImplementedException();
        }

        public RtqMonitoringTaskModel GetTaskDetails(string taskId)
        {
            throw new System.NotImplementedException();
        }

        public Dictionary<string, TaskManipulationResult> CancelTasks(string[] ids)
        {
            throw new System.NotImplementedException();
        }

        public Dictionary<string, TaskManipulationResult> RerunTasks(string[] ids)
        {
            throw new System.NotImplementedException();
        }

        public Dictionary<string, TaskManipulationResult> RerunTasksBySearchQuery(RtqMonitoringSearchRequest searchRequest)
        {
            throw new System.NotImplementedException();
        }

        public Dictionary<string, TaskManipulationResult> CancelTasksBySearchQuery(RtqMonitoringSearchRequest searchRequest)
        {
            throw new System.NotImplementedException();
        }
    }
}