using System;
using System.Collections.Generic;
using System.Linq;

using GroBuf;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Cassandra.Repositories.Indexes.ChildTaskIndex;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using RemoteQueue.Configuration;
using RemoteQueue.Handling.ExecutionContext;
using RemoteQueue.LocalTasks.TaskQueue;
using RemoteQueue.Profiling;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock;
using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock.RemoteLocker;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.Objects.TimeBasedUuid;

using Vostok.Logging.Abstractions;

namespace RemoteQueue.Handling
{
#pragma warning disable 618
    public class RemoteTaskQueue : IRemoteTaskQueue, IRemoteTaskQueueInternals, IRemoteTaskQueueBackdoor
#pragma warning restore 618
    {
        public RemoteTaskQueue(
            ILog logger,
            ISerializer serializer,
            ICassandraCluster cassandraCluster,
            IRemoteTaskQueueSettings taskQueueSettings,
            ITaskDataRegistry taskDataRegistry,
            IRemoteTaskQueueProfiler remoteTaskQueueProfiler)
        {
            TaskTtl = taskQueueSettings.TaskTtl;
            Logger = logger.ForContext("CassandraDistributedTaskQueue");
            Serializer = serializer;
            TaskDataRegistry = taskDataRegistry;
            enableContinuationOptimization = taskQueueSettings.EnableContinuationOptimization;
            ticksHolder = new TicksHolder(cassandraCluster, serializer, taskQueueSettings);
            GlobalTime = new GlobalTime(ticksHolder);
            TaskMinimalStartTicksIndex = new TaskMinimalStartTicksIndex(cassandraCluster, serializer, taskQueueSettings, new OldestLiveRecordTicksHolder(ticksHolder), Logger);
            var taskMetaStorage = new TaskMetaStorage(cassandraCluster, serializer, taskQueueSettings, Logger);
            EventLogRepository = new EventLogRepository(serializer, cassandraCluster, taskQueueSettings, ticksHolder);
            childTaskIndex = new ChildTaskIndex(cassandraCluster, taskQueueSettings, serializer, taskMetaStorage);
            HandleTasksMetaStorage = new HandleTasksMetaStorage(taskMetaStorage, TaskMinimalStartTicksIndex, EventLogRepository, GlobalTime, childTaskIndex, taskDataRegistry, Logger);
            TaskDataStorage = new TaskDataStorage(cassandraCluster, taskQueueSettings, Logger);
            TaskExceptionInfoStorage = new TaskExceptionInfoStorage(cassandraCluster, serializer, taskQueueSettings, Logger);
            HandleTaskCollection = new HandleTaskCollection(HandleTasksMetaStorage, TaskDataStorage, TaskExceptionInfoStorage, remoteTaskQueueProfiler);
            var remoteLockImplementationSettings = CassandraRemoteLockImplementationSettings.Default(taskQueueSettings.QueueKeyspaceForLock, RemoteTaskQueueLockConstants.LockColumnFamily);
            var remoteLockImplementation = new CassandraRemoteLockImplementation(cassandraCluster, serializer, remoteLockImplementationSettings);
            lazyRemoteLockCreator = new Lazy<RemoteLocker>(() => new RemoteLocker(remoteLockImplementation, new RemoteLockerMetrics($"{taskQueueSettings.QueueKeyspaceForLock}_{RemoteTaskQueueLockConstants.LockColumnFamily}"), Logger));
            RemoteTaskQueueProfiler = remoteTaskQueueProfiler;
        }

        public TimeSpan TaskTtl { get; private set; }

        public ILog Logger { get; private set; }
        public ISerializer Serializer { get; private set; }
        public ITaskDataRegistry TaskDataRegistry { get; private set; }
        public IGlobalTime GlobalTime { get; private set; }
        public ITaskMinimalStartTicksIndex TaskMinimalStartTicksIndex { get; private set; }

        [NotNull]
        public EventLogRepository EventLogRepository { get; private set; }

        public IHandleTasksMetaStorage HandleTasksMetaStorage { get; private set; }
        public ITaskDataStorage TaskDataStorage { get; private set; }
        public ITaskExceptionInfoStorage TaskExceptionInfoStorage { get; private set; }
        public IHandleTaskCollection HandleTaskCollection { get; private set; }
        public IRemoteLockCreator RemoteLockCreator => lazyRemoteLockCreator.Value;
        public IRemoteTaskQueueProfiler RemoteTaskQueueProfiler { get; private set; }
        IRemoteTaskQueue IRemoteTaskQueueInternals.RemoteTaskQueue { get { return this; } }

        public TaskManipulationResult TryCancelTask([NotNull] string taskId)
        {
            if (string.IsNullOrWhiteSpace(taskId))
                throw new InvalidProgramStateException("TaskId is required");
            IRemoteLock remoteLock;
            if (!RemoteLockCreator.TryGetLock(taskId, out remoteLock))
                return TaskManipulationResult.Failure_LockAcquiringFails;
            using (remoteLock)
            {
                var task = HandleTaskCollection.TryGetTask(taskId);
                if (task == null)
                    return TaskManipulationResult.Failure_TaskDoesNotExist;
                var taskMeta = task.Meta;
                if (taskMeta.State == TaskState.New || taskMeta.State == TaskState.WaitingForRerun || taskMeta.State == TaskState.WaitingForRerunAfterError || taskMeta.State == TaskState.InProcess)
                {
                    var oldTaskIndexRecord = HandleTasksMetaStorage.FormatIndexRecord(taskMeta);
                    taskMeta.State = TaskState.Canceled;
                    taskMeta.FinishExecutingTicks = Timestamp.Now.Ticks;
                    HandleTasksMetaStorage.AddMeta(taskMeta, oldTaskIndexRecord);
                    return TaskManipulationResult.Success;
                }
                return TaskManipulationResult.Failure_InvalidTaskState;
            }
        }

        public TaskManipulationResult TryRerunTask([NotNull] string taskId, TimeSpan delay)
        {
            if (string.IsNullOrWhiteSpace(taskId))
                throw new InvalidProgramStateException("TaskId is required");
            if (delay.Ticks < 0)
                throw new InvalidProgramStateException(string.Format("Invalid delay: {0}", delay));
            IRemoteLock remoteLock;
            if (!RemoteLockCreator.TryGetLock(taskId, out remoteLock))
                return TaskManipulationResult.Failure_LockAcquiringFails;
            using (remoteLock)
            {
                var task = HandleTaskCollection.TryGetTask(taskId);
                if (task == null)
                    return TaskManipulationResult.Failure_TaskDoesNotExist;
                var taskMeta = task.Meta;
                var oldTaskIndexRecord = HandleTasksMetaStorage.FormatIndexRecord(taskMeta);
                taskMeta.State = TaskState.WaitingForRerun;
                taskMeta.MinimalStartTicks = (Timestamp.Now + delay).Ticks;
                HandleTasksMetaStorage.AddMeta(taskMeta, oldTaskIndexRecord);
                if (taskMeta.NeedTtlProlongation())
                {
                    taskMeta.SetOrUpdateTtl(TaskTtl);
                    HandleTaskCollection.ProlongTaskTtl(taskMeta, task.Data);
                }
                return TaskManipulationResult.Success;
            }
        }

        [CanBeNull]
        public RemoteTaskInfo TryGetTaskInfo([NotNull] string taskId)
        {
            return GetTaskInfos(new[] {taskId}).SingleOrDefault();
        }

        [NotNull]
        public RemoteTaskInfo<T> GetTaskInfo<T>([NotNull] string taskId)
            where T : ITaskData
        {
            var taskInfos = GetTaskInfos<T>(new[] {taskId});
            if (taskInfos.Length == 0)
                throw new InvalidProgramStateException(string.Format("Task {0} does not exist", taskId));
            if (taskInfos.Length > 1)
                throw new InvalidProgramStateException(string.Format("Expected exactly one task info for taskId = {0}, but found {1}", taskId, taskInfos.Length));
            return taskInfos[0];
        }

        [NotNull, ItemNotNull]
        public RemoteTaskInfo[] GetTaskInfos([NotNull, ItemNotNull] string[] taskIds)
        {
            if (taskIds.Any(string.IsNullOrWhiteSpace))
                throw new InvalidProgramStateException(string.Format("Every taskId must be non-empty: {0}", string.Join(", ", taskIds)));
            var tasks = HandleTaskCollection.GetTasks(taskIds);
            var taskExceptionInfos = TaskExceptionInfoStorage.Read(tasks.Select(x => x.Meta).ToArray());
            return tasks.Select(task =>
                {
                    var taskType = TaskDataRegistry.GetTaskType(task.Meta.Name);
                    var taskData = (ITaskData)Serializer.Deserialize(taskType, task.Data);
                    return new RemoteTaskInfo(task.Meta, taskData, taskExceptionInfos[task.Meta.Id]);
                }).ToArray();
        }

        [NotNull, ItemNotNull]
        public RemoteTaskInfo<T>[] GetTaskInfos<T>([NotNull, ItemNotNull] string[] taskIds) where T : ITaskData
        {
            return GetTaskInfos(taskIds).Select(ConvertRemoteTaskInfo<T>).ToArray();
        }

        [NotNull]
        public Dictionary<string, TaskMetaInformation> GetTaskMetas([NotNull, ItemNotNull] string[] taskIds)
        {
            if (taskIds.Any(string.IsNullOrWhiteSpace))
                throw new InvalidProgramStateException(string.Format("Every taskId must be non-empty: {0}", string.Join(", ", taskIds)));
            return HandleTasksMetaStorage.GetMetas(taskIds);
        }

        [NotNull]
        public IRemoteTask CreateTask<T>([NotNull] T taskData, [CanBeNull] CreateTaskOptions createTaskOptions = null) where T : ITaskData
        {
            createTaskOptions = createTaskOptions ?? new CreateTaskOptions();
            var type = taskData.GetType();
            var taskId = TimeGuid.NowGuid().ToGuid().ToString();
            var taskMeta = new TaskMetaInformation(TaskDataRegistry.GetTaskName(type), taskId)
                {
                    Attempts = 0,
                    Ticks = Timestamp.Now.Ticks,
                    ParentTaskId = string.IsNullOrEmpty(createTaskOptions.ParentTaskId) ? GetCurrentExecutingTaskId() : createTaskOptions.ParentTaskId,
                    TaskGroupLock = createTaskOptions.TaskGroupLock,
                    State = TaskState.New,
                    MinimalStartTicks = 0,
                };
            var taskDataBytes = Serializer.Serialize(type, taskData);
            var task = new Task(taskMeta, taskDataBytes);
            return enableContinuationOptimization && LocalTaskQueue.Instance != null
                       ? new RemoteTaskWithContinuationOptimization(task, TaskTtl, HandleTaskCollection, LocalTaskQueue.Instance)
                       : new RemoteTask(task, TaskTtl, HandleTaskCollection);
        }

        [CanBeNull]
        private static string GetCurrentExecutingTaskId()
        {
            var context = TaskExecutionContext.Current;
            if (context == null)
                return null;
            return context.CurrentTask.Meta.Id;
        }

        [NotNull, ItemNotNull]
        public string[] GetChildrenTaskIds([NotNull] string taskId)
        {
            if (string.IsNullOrWhiteSpace(taskId))
                throw new InvalidProgramStateException("TaskId is required");
            return childTaskIndex.GetChildTaskIds(taskId);
        }

        public void ResetTicksHolderInMemoryState()
        {
            ticksHolder.ResetInMemoryState();
        }

        public void ChangeTaskTtl(TimeSpan ttl)
        {
            TaskTtl = ttl;
        }

        [NotNull]
        private static RemoteTaskInfo<T> ConvertRemoteTaskInfo<T>([NotNull] RemoteTaskInfo task) where T : ITaskData
        {
            var taskType = task.TaskData.GetType();
            if (!typeof(T).IsAssignableFrom(taskType))
                throw new Exception(string.Format("Type '{0}' is not assignable from '{1}'", typeof(T).FullName, taskType.FullName));
            return new RemoteTaskInfo<T>(task.Context, (T)task.TaskData, task.ExceptionInfos);
        }

        private readonly TicksHolder ticksHolder;
        private readonly IChildTaskIndex childTaskIndex;
        private readonly bool enableContinuationOptimization;
        private readonly Lazy<RemoteLocker> lazyRemoteLockCreator;
    }
}