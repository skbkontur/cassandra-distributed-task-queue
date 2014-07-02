using System;
using System.Linq;

using GroBuf;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using RemoteQueue.Settings;
using RemoteQueue.UserClasses;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock;
using SKBKontur.Catalogue.CassandraPrimitives.Storages.Primitives;

namespace RemoteQueue.Handling
{
    public class RemoteTaskQueue : IRemoteTaskQueue
    {
        public RemoteTaskQueue(ICassandraCluster cassandraCluster, ICassandraSettings settings, TaskDataRegistryBase taskDataRegistry, ISerializer serializer)
        {
            // ReSharper disable LocalVariableHidesMember
            var parameters = new ColumnFamilyRepositoryParameters(cassandraCluster, settings);
            var ticksHolder = new TicksHolder(serializer, parameters);
            var globalTime = new GlobalTime(ticksHolder);
            var taskMinimalStartTicksIndex = new TaskMinimalStartTicksIndex(parameters, ticksHolder, serializer, globalTime, settings);
            var taskMetaInformationBlobStorage = new TaskMetaInformationBlobStorage(parameters, serializer, globalTime);
            var eventLongRepository = new EventLogRepository(serializer, globalTime, parameters, ticksHolder);
            var handleTasksMetaStorage = new HandleTasksMetaStorage(taskMetaInformationBlobStorage, taskMinimalStartTicksIndex, eventLongRepository, globalTime);
            var handleTaskCollection = new HandleTaskCollection(handleTasksMetaStorage, new TaskDataBlobStorage(parameters, serializer, globalTime));
            var remoteLockCreator = new RemoteLockCreator(new CassandraRemoteLockImplementation(cassandraCluster, serializer, new ColumnFamilyFullName(parameters.Settings.QueueKeyspace, parameters.LockColumnFamilyName)));
            var handleTaskExceptionInfoStorage = new HandleTaskExceptionInfoStorage(new TaskExceptionInfoBlobStorage(parameters, serializer, globalTime));
            InitRemoteTaskQueue(globalTime, serializer, handleTasksMetaStorage, handleTaskCollection, remoteLockCreator, handleTaskExceptionInfoStorage, taskDataRegistry);
            // ReSharper restore LocalVariableHidesMember
        }

        internal RemoteTaskQueue(
            IGlobalTime globalTime,
            ISerializer serializer,
            IHandleTasksMetaStorage handleTasksMetaStorage,
            IHandleTaskCollection handleTaskCollection,
            IRemoteLockCreator remoteLockCreator,
            IHandleTaskExceptionInfoStorage handleTaskExceptionInfoStorage,
            TaskDataRegistryBase taskDataRegistryBase)
        {
            InitRemoteTaskQueue(globalTime, serializer, handleTasksMetaStorage, handleTaskCollection, remoteLockCreator, handleTaskExceptionInfoStorage, taskDataRegistryBase);
        }

        public bool CancelTask(string taskId)
        {
            IRemoteLock remoteLock;
            if(!remoteLockCreator.TryGetLock(taskId, out remoteLock))
                return false;
            using(remoteLock)
            {
                var meta = handleTasksMetaStorage.GetMeta(taskId);
                if(meta.State == TaskState.New || meta.State == TaskState.WaitingForRerun || meta.State == TaskState.WaitingForRerunAfterError || meta.State == TaskState.InProcess)
                {
                    meta.State = TaskState.Canceled;
                    meta.FinishExecutingTicks = DateTime.UtcNow.Ticks;
                    handleTasksMetaStorage.AddMeta(meta);
                    return true;
                }
                return false;
            }
        }

        public bool RerunTask(string taskId, TimeSpan delay)
        {
            IRemoteLock remoteLock;
            if(!remoteLockCreator.TryGetLock(taskId, out remoteLock))
                return false;
            using(remoteLock)
            {
                var meta = handleTasksMetaStorage.GetMeta(taskId);
                meta.State = TaskState.WaitingForRerun;
                meta.MinimalStartTicks = DateTime.UtcNow.Ticks + delay.Ticks;
                handleTasksMetaStorage.AddMeta(meta);
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
            var tasks = handleTaskCollection.GetTasks(taskIds);
            var taskExceptionInfos = handleTaskExceptionInfoStorage.ReadExceptionInfosQuiet(taskIds);
            return tasks.Zip(
                taskExceptionInfos,
                (t, e) =>
                new RemoteTaskInfo
                    {
                        Context = t.Meta,
                        TaskData = (ITaskData)serializer.Deserialize(typeToNameMapper.GetTaskType(t.Meta.Name), t.Data),
                        ExceptionInfo = e
                    }).ToArray();
        }

        public RemoteTaskInfo<T>[] GetTaskInfos<T>(string[] taskIds) where T : ITaskData
        {
            return GetTaskInfos(taskIds).Select(ConvertRemoteTaskInfo<T>).ToArray();
        }

        public IRemoteTask CreateTask<T>(T taskData, CreateTaskOptions createTaskOptions) where T : ITaskData
        {
            createTaskOptions = createTaskOptions ?? new CreateTaskOptions();
            var nowTicks = DateTime.UtcNow.Ticks;
            var taskId = Guid.NewGuid().ToString();
            var type = taskData.GetType();
            var task = new Task
                {
                    Data = serializer.Serialize(type, taskData),
                    Meta = new TaskMetaInformation
                        {
                            Attempts = 0,
                            Id = taskId,
                            Ticks = nowTicks,
                            Name = typeToNameMapper.GetTaskName(type),
                            ParentTaskId = createTaskOptions.ParentTaskId,
                            TaskGroupLock = createTaskOptions.TaskGroupLock,
                            State = TaskState.New,
                        }
                };
            return new RemoteTask(handleTaskCollection, task, globalTime);
        }

        // ReSharper disable ParameterHidesMember
        private void InitRemoteTaskQueue(IGlobalTime globalTime,
                                         ISerializer serializer,
                                         IHandleTasksMetaStorage handleTasksMetaStorage,
                                         IHandleTaskCollection handleTaskCollection,
                                         IRemoteLockCreator remoteLockCreator,
                                         IHandleTaskExceptionInfoStorage handleTaskExceptionInfoStorage,
                                         TaskDataRegistryBase taskDataRegistryBase)
        {
            this.serializer = serializer;
            this.globalTime = globalTime;
            this.handleTasksMetaStorage = handleTasksMetaStorage;
            this.handleTaskCollection = handleTaskCollection;
            typeToNameMapper = new TaskDataTypeToNameMapper(taskDataRegistryBase);
            this.remoteLockCreator = remoteLockCreator;
            this.handleTaskExceptionInfoStorage = handleTaskExceptionInfoStorage;
        }

        // ReSharper restore ParameterHidesMember

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

        private IHandleTaskCollection handleTaskCollection;
        private IHandleTasksMetaStorage handleTasksMetaStorage;
        private ISerializer serializer;
        private ITaskDataTypeToNameMapper typeToNameMapper;
        private IRemoteLockCreator remoteLockCreator;
        private IHandleTaskExceptionInfoStorage handleTaskExceptionInfoStorage;
        private IGlobalTime globalTime;
    }
}