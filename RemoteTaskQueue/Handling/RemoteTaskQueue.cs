using System;
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
using SKBKontur.Catalogue.CassandraPrimitives.Storages.Primitives;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.Objects.TimeBasedUuid;

namespace RemoteQueue.Handling
{
    public class RemoteTaskQueue : IRemoteTaskQueue, IRemoteTaskQueueInternals
    {
        public RemoteTaskQueue(
            ISerializer serializer,
            ICassandraCluster cassandraCluster,
            IRemoteTaskQueueSettings taskQueueSettings,
            ITaskDataRegistry taskDataRegistry,
            IRemoteTaskQueueProfiler remoteTaskQueueProfiler)
        {
            this.taskDataRegistry = taskDataRegistry;
            Serializer = serializer;
            TaskTtl = taskQueueSettings.TaskTtl;
            enableContinuationOptimization = taskQueueSettings.EnableContinuationOptimization;
            TicksHolder = new TicksHolder(cassandraCluster, serializer, taskQueueSettings);
            GlobalTime = new GlobalTime(TicksHolder);
            TaskMinimalStartTicksIndex = new TaskMinimalStartTicksIndex(cassandraCluster, serializer, taskQueueSettings, new OldestLiveRecordTicksHolder(TicksHolder));
            var taskMetaStorage = new TaskMetaStorage(cassandraCluster, serializer, taskQueueSettings);
            var eventLongRepository = new EventLogRepository(serializer, GlobalTime, cassandraCluster, taskQueueSettings, TicksHolder);
            childTaskIndex = new ChildTaskIndex(cassandraCluster, taskQueueSettings, serializer, taskMetaStorage);
            HandleTasksMetaStorage = new HandleTasksMetaStorage(taskMetaStorage, TaskMinimalStartTicksIndex, eventLongRepository, GlobalTime, childTaskIndex, taskDataRegistry);
            taskDataStorage = new TaskDataStorage(cassandraCluster, serializer, taskQueueSettings);
            TaskExceptionInfoStorage = new TaskExceptionInfoStorage(cassandraCluster, serializer, taskQueueSettings);
            HandleTaskCollection = new HandleTaskCollection(HandleTasksMetaStorage, taskDataStorage, TaskExceptionInfoStorage, remoteTaskQueueProfiler);

            var remoteLockImplementationSettings = CassandraRemoteLockImplementationSettings.Default(new ColumnFamilyFullName(taskQueueSettings.QueueKeyspaceForLock, RemoteTaskQueueLockConstants.LockColumnFamily));
            var remoteLockImplementation = new CassandraRemoteLockImplementation(cassandraCluster, serializer, remoteLockImplementationSettings);
            RemoteLockCreator = new RemoteLocker(remoteLockImplementation, new RemoteLockerMetrics(string.Format("{0}_{1}", taskQueueSettings.QueueKeyspaceForLock, RemoteTaskQueueLockConstants.LockColumnFamily)));
            RemoteTaskQueueProfiler = remoteTaskQueueProfiler;
        }

        public TimeSpan TaskTtl { get; private set; }

        public ISerializer Serializer { get; private set; }
        public ITicksHolder TicksHolder { get; private set; }
        public IGlobalTime GlobalTime { get; private set; }
        public ITaskMinimalStartTicksIndex TaskMinimalStartTicksIndex { get; private set; }
        public IHandleTasksMetaStorage HandleTasksMetaStorage { get; private set; }
        public ITaskExceptionInfoStorage TaskExceptionInfoStorage { get; private set; }
        public IHandleTaskCollection HandleTaskCollection { get; private set; }
        public IRemoteLockCreator RemoteLockCreator { get; private set; }
        public IRemoteTaskQueueProfiler RemoteTaskQueueProfiler { get; private set; }
        IRemoteTaskQueue IRemoteTaskQueueInternals.RemoteTaskQueue { get { return this; } }

        public TaskManipulationResult TryCancelTask([NotNull] string taskId)
        {
            IRemoteLock remoteLock;
            if(!RemoteLockCreator.TryGetLock(taskId, out remoteLock))
                return TaskManipulationResult.Unsuccess_LockAcquiringFails;
            using(remoteLock)
            {
                var task = HandleTaskCollection.TryGetTask(taskId);
                if (task == null)
                    return TaskManipulationResult.Unsuccess_TaskDoesNotExist;
                var taskMeta = task.Meta;
                if(taskMeta.State == TaskState.New || taskMeta.State == TaskState.WaitingForRerun || taskMeta.State == TaskState.WaitingForRerunAfterError || taskMeta.State == TaskState.InProcess)
                {
                    var oldTaskIndexRecord = HandleTasksMetaStorage.FormatIndexRecord(taskMeta);
                    taskMeta.State = TaskState.Canceled;
                    taskMeta.FinishExecutingTicks = Timestamp.Now.Ticks;
                    HandleTasksMetaStorage.AddMeta(taskMeta, oldTaskIndexRecord);
                    return TaskManipulationResult.Success;
                }
                return TaskManipulationResult.Unsuccess_InvalidTaskState;
            }
        }

        public TaskManipulationResult TryRerunTask([NotNull] string taskId, TimeSpan delay)
        {
            if(delay.Ticks < 0)
                throw new InvalidProgramStateException(string.Format("Invalid delay: {0}", delay));
            IRemoteLock remoteLock;
            if(!RemoteLockCreator.TryGetLock(taskId, out remoteLock))
                return TaskManipulationResult.Unsuccess_LockAcquiringFails;
            using(remoteLock)
            {
                var task = HandleTaskCollection.TryGetTask(taskId);
                if (task == null)
                    return TaskManipulationResult.Unsuccess_TaskDoesNotExist;
                var taskMeta = task.Meta;
                var oldTaskIndexRecord = HandleTasksMetaStorage.FormatIndexRecord(taskMeta);
                taskMeta.State = TaskState.WaitingForRerun;
                taskMeta.MinimalStartTicks = (Timestamp.Now + delay).Ticks;
                HandleTasksMetaStorage.AddMeta(taskMeta, oldTaskIndexRecord);
                if(taskMeta.NeedTtlProlongation())
                {
                    taskMeta.SetOrUpdateTtl(TaskTtl);
                    HandleTaskCollection.ProlongTaskTtl(taskMeta, task.Data);
                }
                return TaskManipulationResult.Success;
            }
        }

        [NotNull]
        public RemoteTaskInfo GetTaskInfo([NotNull] string taskId)
        {
            return GetTaskInfos(new[] {taskId}).Single();
        }

        [NotNull]
        public RemoteTaskInfo<T> GetTaskInfo<T>([NotNull] string taskId)
            where T : ITaskData
        {
            return GetTaskInfos<T>(new[] {taskId}).Single();
        }

        [NotNull]
        public RemoteTaskInfo[] GetTaskInfos([NotNull] string[] taskIds)
        {
            var tasks = HandleTaskCollection.GetTasks(taskIds);
            var taskExceptionInfos = TaskExceptionInfoStorage.Read(tasks.Select(x => x.Meta).ToArray());
            return tasks.Select(task =>
                {
                    var taskType = taskDataRegistry.GetTaskType(task.Meta.Name);
                    var taskData = (ITaskData)Serializer.Deserialize(taskType, task.Data);
                    return new RemoteTaskInfo(task.Meta, taskData, taskExceptionInfos[task.Meta.Id]);
                }).ToArray();
        }

        [NotNull]
        public RemoteTaskInfo<T>[] GetTaskInfos<T>([NotNull] string[] taskIds) where T : ITaskData
        {
            return GetTaskInfos(taskIds).Select(ConvertRemoteTaskInfo<T>).ToArray();
        }

        [NotNull]
        public IRemoteTask CreateTask<T>([NotNull] T taskData, [CanBeNull] CreateTaskOptions createTaskOptions = null) where T : ITaskData
        {
            createTaskOptions = createTaskOptions ?? new CreateTaskOptions();
            var type = taskData.GetType();
            var taskId = TimeGuid.NowGuid().ToGuid().ToString();
            var taskMeta = new TaskMetaInformation(taskDataRegistry.GetTaskName(type), taskId)
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
            if(context == null)
                return null;
            return context.CurrentTask.Meta.Id;
        }

        [NotNull]
        public string[] GetChildrenTaskIds([NotNull] string taskId)
        {
            return childTaskIndex.GetChildTaskIds(taskId);
        }

        public void ResetTicksHolderInMemoryState()
        {
            TicksHolder.ResetInMemoryState();
        }

        public void ChangeTaskTtl(TimeSpan ttl)
        {
            TaskTtl = ttl;
        }

        [NotNull]
        private static RemoteTaskInfo<T> ConvertRemoteTaskInfo<T>([NotNull] RemoteTaskInfo task) where T : ITaskData
        {
            var taskType = task.TaskData.GetType();
            if(!typeof(T).IsAssignableFrom(taskType))
                throw new Exception(string.Format("Type '{0}' is not assignable from '{1}'", typeof(T).FullName, taskType.FullName));
            return new RemoteTaskInfo<T>(task.Context, (T)task.TaskData, task.ExceptionInfos);
        }

        private readonly ITaskDataRegistry taskDataRegistry;
        private readonly IChildTaskIndex childTaskIndex;
        private readonly TaskDataStorage taskDataStorage;
        private readonly bool enableContinuationOptimization;
    }
}