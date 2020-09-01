using System;
using System.Collections.Generic;
using System.Linq;

using GroBuf;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedLock;
using SkbKontur.Cassandra.DistributedLock.RemoteLocker;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.BlobStorages;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes.ChildTaskIndex;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling.ExecutionContext;
using SkbKontur.Cassandra.DistributedTaskQueue.LocalTasks.TaskQueue;
using SkbKontur.Cassandra.DistributedTaskQueue.Profiling;
using SkbKontur.Cassandra.GlobalTimestamp;
using SkbKontur.Cassandra.ThriftClient.Clusters;
using SkbKontur.Cassandra.TimeBasedUuid;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
{
    [PublicAPI]
    public class RemoteTaskQueue : IRtqTaskProducer, IRtqTaskManager, IRtqInternals
    {
        public RemoteTaskQueue(ILog logger,
                               ISerializer serializer,
                               IGlobalTime globalTime,
                               ICassandraCluster cassandraCluster,
                               IRtqSettings rtqSettings,
                               IRtqTaskDataRegistry taskDataRegistry,
                               IRtqProfiler rtqProfiler)
        {
            QueueKeyspace = rtqSettings.QueueKeyspace;
            TaskTtl = rtqSettings.TaskTtl;
            Logger = logger.ForContext("CassandraDistributedTaskQueue");
            Serializer = serializer;
            GlobalTime = globalTime;
            TaskDataRegistry = taskDataRegistry;
            enableContinuationOptimization = rtqSettings.EnableContinuationOptimization;
            minTicksHolder = new RtqMinTicksHolder(cassandraCluster, rtqSettings);
            TaskMinimalStartTicksIndex = new TaskMinimalStartTicksIndex(cassandraCluster, serializer, rtqSettings, new OldestLiveRecordTicksHolder(minTicksHolder), Logger);
            var taskMetaStorage = new TaskMetaStorage(cassandraCluster, serializer, rtqSettings, Logger);
            EventLogRepository = new EventLogRepository(serializer, cassandraCluster, rtqSettings, minTicksHolder);
            childTaskIndex = new ChildTaskIndex(cassandraCluster, rtqSettings, serializer, taskMetaStorage);
            HandleTasksMetaStorage = new HandleTasksMetaStorage(taskMetaStorage, TaskMinimalStartTicksIndex, EventLogRepository, GlobalTime, childTaskIndex, taskDataRegistry, Logger);
            TaskDataStorage = new TaskDataStorage(cassandraCluster, rtqSettings, Logger);
            TaskExceptionInfoStorage = new TaskExceptionInfoStorage(cassandraCluster, serializer, rtqSettings, Logger);
            HandleTaskCollection = new HandleTaskCollection(HandleTasksMetaStorage, TaskDataStorage, TaskExceptionInfoStorage, rtqProfiler);
            lazyRemoteLockCreator = new Lazy<IRemoteLockCreator>(() =>
                {
                    var remoteLockImplementationSettings = CassandraRemoteLockImplementationSettings.Default(rtqSettings.QueueKeyspace, RtqColumnFamilyRegistry.LocksColumnFamilyName);
                    var remoteLockImplementation = new CassandraRemoteLockImplementation(cassandraCluster, serializer, remoteLockImplementationSettings);
                    var remoteLockerMetrics = new RemoteLockerMetrics($"{rtqSettings.QueueKeyspace}_{RtqColumnFamilyRegistry.LocksColumnFamilyName}");
                    return new RemoteLocker(remoteLockImplementation, remoteLockerMetrics, Logger);
                });
            Profiler = rtqProfiler;
        }

        [NotNull]
        public string QueueKeyspace { get; }

        public TimeSpan TaskTtl { get; private set; }

        public ILog Logger { get; }
        public ISerializer Serializer { get; }
        public IGlobalTime GlobalTime { get; }
        public IRtqTaskDataRegistry TaskDataRegistry { get; }
        public ITaskMinimalStartTicksIndex TaskMinimalStartTicksIndex { get; }

        [NotNull]
        public IEventLogRepository EventLogRepository { get; }

        public IHandleTasksMetaStorage HandleTasksMetaStorage { get; }
        public ITaskDataStorage TaskDataStorage { get; }
        public ITaskExceptionInfoStorage TaskExceptionInfoStorage { get; }
        public IHandleTaskCollection HandleTaskCollection { get; }
        public IRemoteLockCreator RemoteLockCreator => lazyRemoteLockCreator.Value;
        public IRtqProfiler Profiler { get; }

        IRtqTaskProducer IRtqInternals.TaskProducer => this;

        public TaskManipulationResult TryCancelTask([NotNull] string taskId)
        {
            if (string.IsNullOrWhiteSpace(taskId))
                throw new InvalidOperationException("TaskId is required");
            if (!RemoteLockCreator.TryGetLock(taskId, out var remoteLock))
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
                throw new InvalidOperationException("TaskId is required");
            if (delay.Ticks < 0)
                throw new InvalidOperationException(string.Format("Invalid delay: {0}", delay));
            if (!RemoteLockCreator.TryGetLock(taskId, out var remoteLock))
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
            where T : IRtqTaskData
        {
            var taskInfos = GetTaskInfos<T>(new[] {taskId});
            if (taskInfos.Length == 0)
                throw new InvalidOperationException(string.Format("Task {0} does not exist", taskId));
            if (taskInfos.Length > 1)
                throw new InvalidOperationException(string.Format("Expected exactly one task info for taskId = {0}, but found {1}", taskId, taskInfos.Length));
            return taskInfos[0];
        }

        [NotNull, ItemNotNull]
        public RemoteTaskInfo[] GetTaskInfos([NotNull, ItemNotNull] string[] taskIds)
        {
            if (taskIds.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException(string.Format("Every taskId must be non-empty: {0}", string.Join(", ", taskIds)));
            var tasks = HandleTaskCollection.GetTasks(taskIds);
            var taskExceptionInfos = TaskExceptionInfoStorage.Read(tasks.Select(x => x.Meta).ToArray());
            return tasks.Select(task =>
                {
                    var taskType = TaskDataRegistry.GetTaskType(task.Meta.Name);
                    var taskData = (IRtqTaskData)Serializer.Deserialize(taskType, task.Data);
                    return new RemoteTaskInfo(task.Meta, taskData, taskExceptionInfos[task.Meta.Id]);
                }).ToArray();
        }

        [NotNull, ItemNotNull]
        public RemoteTaskInfo<T>[] GetTaskInfos<T>([NotNull, ItemNotNull] string[] taskIds) where T : IRtqTaskData
        {
            return GetTaskInfos(taskIds).Select(ConvertRemoteTaskInfo<T>).ToArray();
        }

        [NotNull]
        public Dictionary<string, TaskMetaInformation> GetTaskMetas([NotNull, ItemNotNull] string[] taskIds)
        {
            if (taskIds.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException(string.Format("Every taskId must be non-empty: {0}", string.Join(", ", taskIds)));
            return HandleTasksMetaStorage.GetMetas(taskIds);
        }

        [NotNull]
        public IRemoteTask CreateTask<T>([NotNull] T taskData, [CanBeNull] CreateTaskOptions createTaskOptions = null) where T : IRtqTaskData
        {
            createTaskOptions ??= new CreateTaskOptions();
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
            return enableContinuationOptimization && localTaskQueue != null
                       ? new RemoteTaskWithContinuationOptimization(task, TaskTtl, HandleTaskCollection, localTaskQueue)
                       : new RemoteTask(task, TaskTtl, HandleTaskCollection);
        }

        [CanBeNull]
        private static string GetCurrentExecutingTaskId()
        {
            return TaskExecutionContext.Current?.CurrentTask.Meta.Id;
        }

        [NotNull, ItemNotNull]
        public string[] GetChildrenTaskIds([NotNull] string taskId)
        {
            if (string.IsNullOrWhiteSpace(taskId))
                throw new InvalidOperationException("TaskId is required");
            return childTaskIndex.GetChildTaskIds(taskId);
        }

        [NotNull, ItemNotNull]
        public string[] GetRecentTaskIds([CanBeNull] Timestamp fromTimestampExclusive, [NotNull] Timestamp toTimestampInclusive)
        {
            if (fromTimestampExclusive >= toTimestampInclusive)
                return new string[0];

            var firstEventTicks = EventLogRepository.GetFirstEventTicks();
            if (firstEventTicks == 0)
                return new string[0];

            var recentTaskIds = new HashSet<string>();
            var exclusiveStartColumnName = fromTimestampExclusive == null || fromTimestampExclusive.Ticks < firstEventTicks
                                               ? EventPointerFormatter.GetMaxColumnNameForTicks(firstEventTicks - 1)
                                               : EventPointerFormatter.GetMaxColumnNameForTimestamp(fromTimestampExclusive);
            var inclusiveEndColumnName = EventPointerFormatter.GetMaxColumnNameForTimestamp(toTimestampInclusive);
            var partitionKey = EventPointerFormatter.GetPartitionKey(EventPointerFormatter.GetTimestamp(exclusiveStartColumnName).Ticks);
            const int eventsToFetch = int.MaxValue - 1;
            while (true)
            {
                var eventsBatch = EventLogRepository.GetEvents(partitionKey, exclusiveStartColumnName, inclusiveEndColumnName, eventsToFetch, out var currentPartitionIsExhausted);
                foreach (var (@event, _) in eventsBatch)
                    recentTaskIds.Add(@event.TaskId);

                if (!currentPartitionIsExhausted)
                    throw new InvalidOperationException("currentPartitionIsExhausted == false");

                var nextPartitionStartTicks = (EventPointerFormatter.ParsePartitionKey(partitionKey) + 1) * EventPointerFormatter.PartitionDurationTicks;
                if (nextPartitionStartTicks > toTimestampInclusive.Ticks)
                    return recentTaskIds.ToArray();

                partitionKey = EventPointerFormatter.GetPartitionKey(nextPartitionStartTicks);
                exclusiveStartColumnName = EventPointerFormatter.GetMaxColumnNameForTicks(nextPartitionStartTicks - 1);
            }
        }

        void IRtqInternals.AttachLocalTaskQueue([NotNull] LocalTaskQueue localTaskQueueInstance)
        {
            localTaskQueue = localTaskQueueInstance;
        }

        void IRtqInternals.ResetTicksHolderInMemoryState()
        {
            GlobalTime.ResetInMemoryState();
            minTicksHolder.ResetInMemoryState();
        }

        void IRtqInternals.ChangeTaskTtl(TimeSpan ttl)
        {
            TaskTtl = ttl;
        }

        [NotNull]
        private static RemoteTaskInfo<T> ConvertRemoteTaskInfo<T>([NotNull] RemoteTaskInfo task) where T : IRtqTaskData
        {
            var taskType = task.TaskData.GetType();
            if (!typeof(T).IsAssignableFrom(taskType))
                throw new InvalidOperationException(string.Format("Type '{0}' is not assignable from '{1}'", typeof(T).FullName, taskType.FullName));
            return new RemoteTaskInfo<T>(task.Context, (T)task.TaskData, task.ExceptionInfos);
        }

        [CanBeNull]
        private LocalTaskQueue localTaskQueue;

        private readonly IMinTicksHolder minTicksHolder;
        private readonly IChildTaskIndex childTaskIndex;
        private readonly bool enableContinuationOptimization;
        private readonly Lazy<IRemoteLockCreator> lazyRemoteLockCreator;
    }
}