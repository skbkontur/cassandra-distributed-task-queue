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
using RemoteQueue.Handling.ExecutionContext;
using RemoteQueue.LocalTasks.TaskQueue;
using RemoteQueue.Profiling;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock;
using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock.RemoteLocker;
using SKBKontur.Catalogue.CassandraPrimitives.Storages.Primitives;

namespace RemoteQueue.Handling
{
    public class RemoteTaskQueue : IRemoteTaskQueue, IRemoteTaskQueueInternals
    {
        public RemoteTaskQueue(
            ISerializer serializer,
            ICassandraCluster cassandraCluster,
            ICassandraSettings cassandraSettings,
            IRemoteTaskQueueSettings taskQueueSettings,
            ITaskDataTypeToNameMapper taskDataTypeToNameMapper,
            IRemoteTaskQueueProfiler remoteTaskQueueProfiler)
        {
            Serializer = serializer;
            this.taskDataTypeToNameMapper = taskDataTypeToNameMapper;
            enableContinuationOptimization = taskQueueSettings.EnableContinuationOptimization;
            var parameters = new ColumnFamilyRepositoryParameters(cassandraCluster, cassandraSettings);
            var ticksHolder = new TicksHolder(serializer, parameters);
            GlobalTime = new GlobalTime(ticksHolder);
            TaskMinimalStartTicksIndex = new TaskMinimalStartTicksIndex(parameters, serializer, GlobalTime, new FromTicksProvider(new OldestLiveRecordTicksHolder(ticksHolder)));
            var taskMetaInformationBlobStorage = new TaskMetaInformationBlobStorage(parameters, serializer, GlobalTime);
            var eventLongRepository = new EventLogRepository(serializer, GlobalTime, parameters, ticksHolder);
            childTaskIndex = new ChildTaskIndex(parameters, serializer, taskMetaInformationBlobStorage);
            HandleTasksMetaStorage = new HandleTasksMetaStorage(taskMetaInformationBlobStorage, TaskMinimalStartTicksIndex, eventLongRepository, GlobalTime, childTaskIndex);
            HandleTaskCollection = new HandleTaskCollection(HandleTasksMetaStorage, new TaskDataBlobStorage(parameters, serializer, GlobalTime), remoteTaskQueueProfiler);
            HandleTaskExceptionInfoStorage = new HandleTaskExceptionInfoStorage(new TaskExceptionInfoBlobStorage(parameters, serializer, GlobalTime));
            var remoteLockImplementationSettings = CassandraRemoteLockImplementationSettings.Default(new ColumnFamilyFullName(parameters.Settings.QueueKeyspace, parameters.LockColumnFamilyName));
            var remoteLockImplementation = new CassandraRemoteLockImplementation(cassandraCluster, serializer, remoteLockImplementationSettings);
            RemoteLockCreator = taskQueueSettings.UseRemoteLocker ? (IRemoteLockCreator)new RemoteLocker(remoteLockImplementation, new RemoteLockerMetrics(parameters.Settings.QueueKeyspace)) : new RemoteLockCreator(remoteLockImplementation);
            RemoteTaskQueueProfiler = remoteTaskQueueProfiler;
        }

        public ISerializer Serializer { get; private set; }
        public IGlobalTime GlobalTime { get; private set; }
        public ITaskMinimalStartTicksIndex TaskMinimalStartTicksIndex { get; private set; }
        public IHandleTasksMetaStorage HandleTasksMetaStorage { get; private set; }
        public IHandleTaskCollection HandleTaskCollection { get; private set; }
        public IRemoteLockCreator RemoteLockCreator { get; private set; }
        public IHandleTaskExceptionInfoStorage HandleTaskExceptionInfoStorage { get; private set; }
        public IRemoteTaskQueueProfiler RemoteTaskQueueProfiler { get; private set; }
        IRemoteTaskQueue IRemoteTaskQueueInternals.RemoteTaskQueue { get { return this; } }

        public bool CancelTask(string taskId)
        {
            IRemoteLock remoteLock;
            if(!RemoteLockCreator.TryGetLock(taskId, out remoteLock))
                return false;
            using(remoteLock)
            {
                var meta = HandleTasksMetaStorage.GetMeta(taskId);
                if(meta.State == TaskState.New || meta.State == TaskState.WaitingForRerun || meta.State == TaskState.WaitingForRerunAfterError || meta.State == TaskState.InProcess)
                {
                    meta.State = TaskState.Canceled;
                    meta.FinishExecutingTicks = DateTime.UtcNow.Ticks;
                    HandleTasksMetaStorage.AddMeta(meta);
                    return true;
                }
                return false;
            }
        }

        public bool RerunTask(string taskId, TimeSpan delay)
        {
            IRemoteLock remoteLock;
            if(!RemoteLockCreator.TryGetLock(taskId, out remoteLock))
                return false;
            using(remoteLock)
            {
                var meta = HandleTasksMetaStorage.GetMeta(taskId);
                meta.State = TaskState.WaitingForRerun;
                meta.MinimalStartTicks = DateTime.UtcNow.Ticks + delay.Ticks;
                HandleTasksMetaStorage.AddMeta(meta);
                return true;
            }
        }

        public RemoteTaskInfo GetTaskInfo(string taskId)
        {
            return GetTaskInfos(new[] {taskId}).First();
        }

        public RemoteTaskInfo<T> GetTaskInfo<T>(string taskId)
            where T : ITaskData
        {
            return GetTaskInfos<T>(new[] {taskId}).First();
        }

        public RemoteTaskInfo[] GetTaskInfos(string[] taskIds)
        {
            var tasks = HandleTaskCollection.GetTasks(taskIds);
            var taskExceptionInfos = HandleTaskExceptionInfoStorage.ReadExceptionInfosQuiet(taskIds);
            return tasks.Zip(
                taskExceptionInfos,
                (t, e) =>
                new RemoteTaskInfo
                    {
                        Context = t.Meta,
                        TaskData = (ITaskData)Serializer.Deserialize(taskDataTypeToNameMapper.GetTaskType(t.Meta.Name), t.Data),
                        ExceptionInfo = e
                    }).ToArray();
        }

        public RemoteTaskInfo<T>[] GetTaskInfos<T>(string[] taskIds) where T : ITaskData
        {
            return GetTaskInfos(taskIds).Select(ConvertRemoteTaskInfo<T>).ToArray();
        }

        [NotNull]
        public IRemoteTask CreateTask<T>(T taskData, CreateTaskOptions createTaskOptions) where T : ITaskData
        {
            createTaskOptions = createTaskOptions ?? new CreateTaskOptions();
            var nowTicks = DateTime.UtcNow.Ticks;
            var taskId = Guid.NewGuid().ToString();
            var type = taskData.GetType();
            var task = new Task
                {
                    Data = Serializer.Serialize(type, taskData),
                    Meta = new TaskMetaInformation
                        {
                            Attempts = 0,
                            Id = taskId,
                            Ticks = nowTicks,
                            Name = taskDataTypeToNameMapper.GetTaskName(type),
                            ParentTaskId = string.IsNullOrEmpty(createTaskOptions.ParentTaskId) ? GetCurrentExecutingTaskId() : createTaskOptions.ParentTaskId,
                            TaskGroupLock = createTaskOptions.TaskGroupLock,
                            State = TaskState.New,
                            MinimalStartTicks = 0,
                        }
                };
            return enableContinuationOptimization && LocalTaskQueue.Instance != null
                       ? new RemoteTaskWithContinuationOptimization(task, HandleTaskCollection, LocalTaskQueue.Instance)
                       : new RemoteTask(task, HandleTaskCollection);
        }

        private static string GetCurrentExecutingTaskId()
        {
            var context = TaskExecutionContext.Current;
            if(context == null)
                return null;
            return context.CurrentTask.Meta.Id;
        }

        public string[] GetChildrenTaskIds(string taskId)
        {
            return childTaskIndex.GetChildTaskIds(taskId);
        }

        private static RemoteTaskInfo<T> ConvertRemoteTaskInfo<T>(RemoteTaskInfo task) where T : ITaskData
        {
            var taskType = task.TaskData.GetType();
            if(!typeof(T).IsAssignableFrom(taskType))
                throw new Exception(string.Format("Type '{0}' is not assignable from '{1}'", typeof(T).FullName, taskType.FullName));
            return new RemoteTaskInfo<T>
                {
                    Context = task.Context,
                    TaskData = (T)task.TaskData,
                    ExceptionInfo = task.ExceptionInfo
                };
        }

        private readonly ITaskDataTypeToNameMapper taskDataTypeToNameMapper;
        private readonly IChildTaskIndex childTaskIndex;
        private readonly bool enableContinuationOptimization;
    }
}