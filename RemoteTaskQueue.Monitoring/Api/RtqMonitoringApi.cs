using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage.Client;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api
{
    public class RtqMonitoringApi : IRtqMonitoringApi
    {
        public RtqMonitoringApi(IRtqTaskManager taskManager, TaskSearchClient taskSearchClient, IRtqTaskDataRegistry taskDataRegistry)
        {
            this.taskManager = taskManager;
            this.taskSearchClient = taskSearchClient;
            this.taskDataRegistry = taskDataRegistry;
        }

        [NotNull, ItemNotNull]
        public string[] GetAllTasksNames()
        {
            return taskDataRegistry.GetAllTaskNames();
        }

        [NotNull]
        public RtqMonitoringSearchResults Search([NotNull] RtqMonitoringSearchRequest searchRequest, int from, int size)
        {
            if (searchRequest.EnqueueDateTimeRange == null)
                throw new ArgumentException("enqueueDateTimeRange should be specified");
            if (searchRequest.EnqueueDateTimeRange.OpenType != null)
                throw new ArgumentException("Both enqueueDateTimeRange.lowerBound and enqueueDateTimeRange.uppedBound should be specified");

            var searchResult = FindTasks(searchRequest, from, size);
            var taskMetas = taskManager.GetTaskMetas(searchResult.Ids);
            var taskListItems = new List<TaskMetaInformation>();
            foreach (var taskId in searchResult.Ids)
            {
                if (taskMetas.TryGetValue(taskId, out var taskMeta))
                    taskListItems.Add(taskMeta);
            }
            return new RtqMonitoringSearchResults
                {
                    TotalCount = searchResult.TotalCount,
                    TaskMetas = taskListItems.ToArray(),
                };
        }

        [NotNull]
        private TaskSearchResponse FindTasks([NotNull] RtqMonitoringSearchRequest searchRequest, int from, int size)
        {
            return taskSearchClient.Search(CreateTaskSearchRequest(searchRequest), from, size);
        }

        private IEnumerable<string> FindAllTasks([NotNull] RtqMonitoringSearchRequest searchRequest)
        {
            const int batchSize = 100;
            var taskSearchRequest = CreateTaskSearchRequest(searchRequest);
            var currentOffset = 0;
            while (true)
            {
                var result = taskSearchClient.Search(taskSearchRequest, currentOffset, batchSize);
                if (result.Ids.Length == 0)
                    yield break;
                foreach (var id in result.Ids)
                    yield return id;
                currentOffset += batchSize;
            }
        }

        [NotNull]
        private static TaskSearchRequest CreateTaskSearchRequest([NotNull] RtqMonitoringSearchRequest searchRequest)
        {
            return new TaskSearchRequest
                {
                    TaskStates = searchRequest.States?.Select(x => x.ToString()).ToArray(),
                    TaskNames = searchRequest.Names,
                    QueryString = string.IsNullOrWhiteSpace(searchRequest.QueryString) ? "*" : searchRequest.QueryString,
                    FromTicksUtc = searchRequest.EnqueueDateTimeRange.LowerBound.Ticks,
                    ToTicksUtc = searchRequest.EnqueueDateTimeRange.UpperBound.Ticks,
                };
        }

        [CanBeNull]
        public RtqMonitoringTaskModel GetTaskDetails([NotNull] string taskId)
        {
            var result = taskManager.TryGetTaskInfo(taskId);
            if (result == null)
                return null;

            return new RtqMonitoringTaskModel
                {
                    ExceptionInfos = result.ExceptionInfos,
                    TaskData = result.TaskData,
                    TaskMeta = result.Context,
                    ChildTaskIds = taskManager.GetChildrenTaskIds(taskId),
                };
        }

        [NotNull]
        public Dictionary<string, TaskManipulationResult> CancelTasks([NotNull, ItemNotNull] string[] ids)
        {
            var result = new Dictionary<string, TaskManipulationResult>();
            foreach (var taskId in ids.Distinct())
            {
                var taskManipulationResult = taskManager.TryCancelTask(taskId);
                result.Add(taskId, taskManipulationResult);
            }
            return result;
        }

        [NotNull]
        public Dictionary<string, TaskManipulationResult> RerunTasks([NotNull, ItemNotNull] string[] ids)
        {
            var result = new Dictionary<string, TaskManipulationResult>();
            foreach (var taskId in ids.Distinct())
            {
                var taskManipulationResult = taskManager.TryRerunTask(taskId, TimeSpan.Zero);
                result.Add(taskId, taskManipulationResult);
            }
            return result;
        }

        [NotNull]
        public Dictionary<string, TaskManipulationResult> RerunTasksBySearchQuery([NotNull] RtqMonitoringSearchRequest searchRequest)
        {
            var result = new Dictionary<string, TaskManipulationResult>();
            foreach (var taskId in FindAllTasks(searchRequest).Distinct())
            {
                var taskManipulationResult = taskManager.TryRerunTask(taskId, TimeSpan.Zero);
                result.Add(taskId, taskManipulationResult);
            }
            return result;
        }

        [NotNull]
        public Dictionary<string, TaskManipulationResult> CancelTasksBySearchQuery([NotNull] RtqMonitoringSearchRequest searchRequest)
        {
            var result = new Dictionary<string, TaskManipulationResult>();
            foreach (var taskId in FindAllTasks(searchRequest).Distinct())
            {
                var taskManipulationResult = taskManager.TryCancelTask(taskId);
                result.Add(taskId, taskManipulationResult);
            }
            return result;
        }

        public void ResetTicksHolderInMemoryState()
        {
            ((RemoteTaskQueue)taskManager).ResetTicksHolderInMemoryState();
        }

        private readonly IRtqTaskManager taskManager;
        private readonly TaskSearchClient taskSearchClient;
        private readonly IRtqTaskDataRegistry taskDataRegistry;
    }
}