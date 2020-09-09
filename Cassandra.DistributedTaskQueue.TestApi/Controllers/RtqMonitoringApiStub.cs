using System;
using System.Collections.Generic;
using System.Linq;

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
                    ExceptionInfos = new string[0],
                    TaskData = new TaskData
                        {
                            Date = DateTime.Now,
                            Number = 200,
                            TestEnum = TestEnum.Value1,
                            DocumentCirculationId = Guid.NewGuid().ToString()
                        },
                    TaskMeta = tasks[taskId],
                    ChildTaskIds = new string[0],
                };
        }

        public Dictionary<string, TaskManipulationResult> CancelTasks(string[] ids)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, TaskManipulationResult> RerunTasks(string[] ids)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, TaskManipulationResult> RerunTasksBySearchQuery(RtqMonitoringSearchRequest searchRequest)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, TaskManipulationResult> CancelTasksBySearchQuery(RtqMonitoringSearchRequest searchRequest)
        {
            throw new NotImplementedException();
        }

        private readonly Dictionary<string, RtqMonitoringTaskMeta> tasks = new Dictionary<string, RtqMonitoringTaskMeta>
            {
                {"id1", new RtqMonitoringTaskMeta {Id = "id1", Name = "Task1"}},
                {"id2", new RtqMonitoringTaskMeta {Id = "id2", Name = "Task2"}}
            };
    }
}