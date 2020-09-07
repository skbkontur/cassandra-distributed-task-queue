using System.Collections.Generic;
using System.Linq;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api;

namespace SkbKontur.Cassandra.DistributedTaskQueue.TestApi.Controllers
{
    public class RtqMonitoringApiStub : IRtqMonitoringApi
    {
        public string[] GetAllTasksNames()
        {
            return new[] {"Name1", "Name2"};
        }

        public RtqMonitoringSearchResults Search(RtqMonitoringSearchRequest searchRequest)
        {
            return new RtqMonitoringSearchResults
                {
                    TaskMetas = tasks.Values.ToArray(),
                    TotalCount = tasks.Count,
                };
        }

        public RtqMonitoringTaskModel GetTaskDetails(string taskId)
        {
            return new RtqMonitoringTaskModel
                {
                    ExceptionInfos = new TaskExceptionInfo[0],
                    TaskData = new TaskData(),
                    TaskMeta = tasks[taskId],
                    ChildTaskIds = new string[0],
                };
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

        private readonly Dictionary<string, TaskMetaInformation> tasks = new Dictionary<string, TaskMetaInformation>
            {
                {"id1", new TaskMetaInformation("Task1", "id1")},
                {"id2", new TaskMetaInformation("Task2", "id2")}
            };
    }
}