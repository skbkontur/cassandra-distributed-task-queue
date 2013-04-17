using System;

using GroBuf;

using RemoteLock;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using RemoteQueue.Settings;
using RemoteQueue.UserClasses;

using SKBKontur.Cassandra.CassandraClient.Clusters;

namespace RemoteQueue.Handling
{
    public class RemoteTaskQueue : IRemoteTaskQueue
    {
        public RemoteTaskQueue(ICassandraSettings settings, TaskDataRegistryBase taskDataRegistry)
        {
            serializer = StaticGrobuf.GetSerializer();
            var cassandraCluster = new CassandraCluster(settings);
            var parameters = new ColumnFamilyRepositoryParameters(cassandraCluster, settings);
            var ticksHolder = new TicksHolder(serializer, parameters);
            var globalTime = new GlobalTime(ticksHolder);
            var taskMinimalStartTicksIndex = new TaskMinimalStartTicksIndex(parameters, ticksHolder, serializer, globalTime, settings);
            var taskMetaInformationBlobStorage = new TaskMetaInformationBlobStorage(parameters, serializer, globalTime);
            var eventLongRepository = new EventLogRepository(serializer, globalTime, parameters, ticksHolder);
            handleTasksMetaStorage = new HandleTasksMetaStorage(taskMetaInformationBlobStorage, taskMinimalStartTicksIndex, eventLongRepository, globalTime);
            handleTaskCollection = new HandleTaskCollection(handleTasksMetaStorage, new TaskDataBlobStorage(parameters, serializer, globalTime));
            typeToNameMapper = new TaskDataTypeToNameMapper(taskDataRegistry);
            remoteLockCreator = new RemoteLockCreator(new CassandraRemoteLockImplementation(cassandraCluster, parameters.Settings, serializer, parameters.Settings.QueueKeyspace, parameters.LockColumnFamilyName));
            handleTaskExceptionInfoStorage = new HandleTaskExceptionInfoStorage(new TaskExceptionInfoBlobStorage(parameters, serializer, globalTime));
        }

        public bool CancelTask(string taskId)
        {
            IRemoteLock remoteLock;
            if(!remoteLockCreator.TryGetLock(taskId, out remoteLock))
                return false;
            using(remoteLock)
            {
                var meta = handleTasksMetaStorage.GetMeta(taskId);
                if(meta.State == TaskState.New || meta.State == TaskState.WaitingForRerun || meta.State == TaskState.WaitingForRerunAfterError)
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
            var task = handleTaskCollection.GetTask(taskId);
            var res = (ITaskData)serializer.Deserialize(typeToNameMapper.GetTaskType(task.Meta.Name), task.Data);
            TaskExceptionInfo info;
            handleTaskExceptionInfoStorage.TryGetExceptionInfo(taskId, out info);
            return new RemoteTaskInfo
                {
                    Context = task.Meta,
                    TaskData = res,
                    ExceptionInfo = info
                };
        }

        public RemoteTaskInfo<T> GetTaskInfo<T>(string taskId)
            where T : ITaskData
        {
            var task = handleTaskCollection.GetTask(taskId);
            var taskType = typeToNameMapper.GetTaskType(task.Meta.Name);
            if(!typeof(T).IsAssignableFrom(taskType))
                throw new Exception(string.Format("Type '{0}' is not assignable from '{1}'", typeof(T).FullName, taskType.FullName));
            var res = (T)serializer.Deserialize(taskType, task.Data);
            TaskExceptionInfo info;
            handleTaskExceptionInfoStorage.TryGetExceptionInfo(taskId, out info);
            return new RemoteTaskInfo<T>
                {
                    Context = task.Meta,
                    TaskData = res,
                    ExceptionInfo = info
                };
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
            return new RemoteTask(handleTaskCollection, task);
        }

        private readonly IHandleTaskCollection handleTaskCollection;
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly ISerializer serializer;
        private readonly ITaskDataTypeToNameMapper typeToNameMapper;
        private readonly IRemoteLockCreator remoteLockCreator;
        private readonly HandleTaskExceptionInfoStorage handleTaskExceptionInfoStorage;
    }
}