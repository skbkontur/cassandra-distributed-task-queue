using System;
using System.Collections.Generic;
using System.Linq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Configuration;
using RemoteQueue.Handling;

using RemoteTaskQueue.Monitoring.Storage.Client;

using SKBKontur.Catalogue.Core.InternalApi.Core;
using SKBKontur.Catalogue.Core.InternalApi.Core.Exceptions;
using SKBKontur.Catalogue.Objects;

namespace RemoteTaskQueue.Monitoring.Api
{
    public class RemoteTaskQueueMonitoringApi : IRemoteTaskQueueMonitoringApi
    {
        public RemoteTaskQueueMonitoringApi(IRemoteTaskQueue remoteTaskQueue, ITaskSearchClient taskSearchClient, ITaskDataRegistry taskDataRegistry)
        {
            this.remoteTaskQueue = remoteTaskQueue;
            this.taskSearchClient = taskSearchClient;
            this.taskDataRegistry = taskDataRegistry;
        }

        public string[] GetAllTaksNames()
        {
            return taskDataRegistry.GetAllTaskNames();
        }

        public RemoteTaskQueueSearchResults Search(RemoteTaskQueueSearchRequest searchRequest, int from, int size)
        {
            if(searchRequest.EnqueueDateTimeRange == null)
                throw new BadRequestException("enqueueDateTimeRange should be specified");
            if(searchRequest.EnqueueDateTimeRange.OpenType != null)
                throw new BadRequestException("Both enqueueDateTimeRange.lowerBound and enqueueDateTimeRange.uppedBound should be specified");

            var searchResult = FindTasks(searchRequest, from, size);
            var taskMetas = remoteTaskQueue.GetTaskMetas(searchResult.Ids);
            var taskListItems = new List<TaskMetaInformation>();
            foreach(var taskId in searchResult.Ids)
            {
                TaskMetaInformation taskMeta;
                if(taskMetas.TryGetValue(taskId, out taskMeta))
                    taskListItems.Add(taskMeta);
            }
            return new RemoteTaskQueueSearchResults
                {
                    TotalCount = searchResult.TotalCount,
                    TaskMetas = taskListItems.ToArray(),
                };
        }

        private TaskSearchResponse FindTasks(RemoteTaskQueueSearchRequest searchRequest, int from, int size)
        {
            return taskSearchClient.Search(CreateTaskSearchRequest(searchRequest), from, size);
        }

        private IEnumerable<string> FindAllTasks(RemoteTaskQueueSearchRequest searchRequest)
        {
            const int batchSize = 100;
            var taskSearchRequest = CreateTaskSearchRequest(searchRequest);
            var currentOffset = 0;
            while(true)
            {
                var result = taskSearchClient.Search(taskSearchRequest, currentOffset, batchSize);
                if(result.Ids.Length == 0)
                    yield break;
                foreach(var id in result.Ids)
                    yield return id;
                currentOffset += batchSize;
            }
        }

        private static TaskSearchRequest CreateTaskSearchRequest(RemoteTaskQueueSearchRequest searchRequest)
        {
            return new TaskSearchRequest
                {
                    TaskStates = searchRequest.With(x => x.States).Return(z => z.Select(x => x.ToString()).ToArray(), null),
                    TaskNames = searchRequest.Return(x => x.Names, null),
                    QueryString = searchRequest.With(x => x.QueryString).Unless(string.IsNullOrWhiteSpace).Return(x => x, "*"),
                    FromTicksUtc = searchRequest.EnqueueDateTimeRange.LowerBound.Ticks,
                    ToTicksUtc = searchRequest.EnqueueDateTimeRange.UpperBound.Ticks,
                };
        }

//        [NotNull]
//        private static TaskMetaInformation TaskMetaToApiModel([NotNull] TaskMetaInformation taskMeta, [CanBeNull] string[] childrenTaskIds)
//        {
//            return new TaskMetaInformationModel
//                {
//                    Id = taskMeta.Id,
//                    Name = taskMeta.Name,
//                    EnqueueDateTime = TicksToDateTime(taskMeta.Ticks),
//                    MinimalStartDateTime = TicksToDateTime(taskMeta.MinimalStartTicks),
//                    StartExecutingDateTime = TicksToDateTime(taskMeta.StartExecutingTicks),
//                    FinishExecutingDateTime = TicksToDateTime(taskMeta.FinishExecutingTicks),
//                    LastModificationDateTime = TicksToDateTime(taskMeta.LastModificationTicks),
//                    ExpirationTimestamp = TicksToDateTime(taskMeta.ExpirationTimestampTicks),
//                    ExpirationModificationDateTime = TicksToDateTime(taskMeta.ExpirationModificationTicks),
//                    ChildTaskIds = childrenTaskIds,
//                    State = taskMeta.State,
//                    Attempts = taskMeta.Attempts,
//                    ParentTaskId = taskMeta.ParentTaskId,
//                    TaskGroupLock = taskMeta.TaskGroupLock,
//                    TraceId = taskMeta.TraceId,
//                    TraceIsActive = taskMeta.TraceIsActive,
//                };
//        }

        public RemoteTaskInfoModel GetTaskDetails(string taskId)
        {
            var result = remoteTaskQueue.TryGetTaskInfo(taskId);
            if(result == null)
                throw new NotFoundException(string.Format("Task with id {0} not found", taskId));
            return new RemoteTaskInfoModel
                {
                    ExceptionInfos = result.ExceptionInfos,
                    TaskData = result.TaskData,
                    TaskMeta = new Merged<TaskMetaInformation, TaskMetaInformationChildTasks>(
                        result.Context,
                        new TaskMetaInformationChildTasks
                            {
                                ChildTaskIds = remoteTaskQueue.GetChildrenTaskIds(taskId)
                            }
                        ),
                };
        }

        public Dictionary<string, TaskManipulationResult> CancelTasks(string[] ids)
        {
            var result = new Dictionary<string, TaskManipulationResult>();
            foreach(var taskId in ids.Distinct())
            {
                var taskManipulationResult = remoteTaskQueue.TryCancelTask(taskId);
                result.Add(taskId, taskManipulationResult);
            }
            return result;
        }

        public Dictionary<string, TaskManipulationResult> RerunTasks(string[] ids)
        {
            var result = new Dictionary<string, TaskManipulationResult>();
            foreach(var taskId in ids.Distinct())
            {
                var taskManipulationResult = remoteTaskQueue.TryRerunTask(taskId, TimeSpan.Zero);
                result.Add(taskId, taskManipulationResult);
            }
            return result;
        }

        public Dictionary<string, TaskManipulationResult> RerunTasksBySearchQuery(RemoteTaskQueueSearchRequest searchRequest)
        {
            var result = new Dictionary<string, TaskManipulationResult>();
            foreach(var taskId in FindAllTasks(searchRequest).Distinct())
            {
                var taskManipulationResult = remoteTaskQueue.TryRerunTask(taskId, TimeSpan.Zero);
                result.Add(taskId, taskManipulationResult);
            }
            return result;
        }

        public Dictionary<string, TaskManipulationResult> CancelTasksBySearchQuery(RemoteTaskQueueSearchRequest searchRequest)
        {
            var result = new Dictionary<string, TaskManipulationResult>();
            foreach(var taskId in FindAllTasks(searchRequest).Distinct())
            {
                var taskManipulationResult = remoteTaskQueue.TryCancelTask(taskId);
                result.Add(taskId, taskManipulationResult);
            }
            return result;
        }

        public void ResetTicksHolderInMemoryState()
        {
            (remoteTaskQueue as RemoteQueue.Handling.RemoteTaskQueue).ResetTicksHolderInMemoryState();
        }

        private readonly IRemoteTaskQueue remoteTaskQueue;
        private readonly ITaskSearchClient taskSearchClient;
        private readonly ITaskDataRegistry taskDataRegistry;
    }
}