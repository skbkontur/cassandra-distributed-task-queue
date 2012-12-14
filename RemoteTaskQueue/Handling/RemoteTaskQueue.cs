using System;

using GroBuf;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.RemoteLock;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Cassandra.Repositories.Indexes.EventIndexes;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using RemoteQueue.Settings;
using RemoteQueue.UserClasses;

using SKBKontur.Cassandra.CassandraClient.Clusters;

namespace RemoteQueue.Handling
{
    public class RemoteTaskQueue : IRemoteTaskQueue
    {
        public RemoteTaskQueue(ICassandraSettings settings, ITaskDataRegistry taskDataRegistry)
        {
            serializer = StaticGrobuf.GetSerializer();
            var cassandraCluster = new CassandraCluster(settings);
            var parameters = new ColumnFamilyRepositoryParameters(cassandraCluster, settings);
            var ticksHolder = new TicksHolder(serializer, parameters);
            var globalTime = new GlobalTime(ticksHolder);
            var taskMetaEventColumnInfoIndex = new TaskMetaEventColumnInfoIndex(serializer, globalTime, parameters);
            var taskMinimalStartTicksIndex = new TaskMinimalStartTicksIndex(parameters, taskMetaEventColumnInfoIndex, new IndexRecordsCleaner(parameters, taskMetaEventColumnInfoIndex, serializer, globalTime), ticksHolder, serializer, globalTime, settings);
            var taskMetaInformationBlobStorage = new TaskMetaInformationBlobStorage(parameters, serializer, globalTime);
            handleTasksMetaStorage = new HandleTasksMetaStorage(taskMetaInformationBlobStorage, taskMinimalStartTicksIndex);
            handleTaskCollection = new HandleTaskCollection(handleTasksMetaStorage, new TaskDataBlobStorage(parameters, serializer, globalTime));
            typeToNameMapper = new TaskDataTypeToNameMapper(taskDataRegistry);
            remoteLockCreator = new RemoteLockCreator(new LockRepository(parameters));
        }

        //public RemoteTaskQueue(
        //    IHandleTaskCollection handleTaskCollection,
        //    IHandleTasksMetaStorage handleTasksMetaStorage,
        //    SerializerWrapper serializer,
        //    ITaskDataTypeToNameMapper typeToNameMapper,
        //    IRemoteLockCreator remoteLockCreator)
        //{
        //    this.handleTaskCollection = handleTaskCollection;
        //    this.handleTasksMetaStorage = handleTasksMetaStorage;
        //    this.serializer = serializer;
        //    this.typeToNameMapper = typeToNameMapper;
        //    this.remoteLockCreator = remoteLockCreator;
        //}

        public bool CancelTask(string taskId)
        {
            IRemoteLock remoteLock;
            if(!remoteLockCreator.TryGetLock(taskId, out remoteLock))
                return false;
            using(remoteLock)
            {
                TaskMetaInformation meta = handleTasksMetaStorage.GetMeta(taskId);
                if(meta.State == TaskState.New || meta.State == TaskState.WaitingForRerun || meta.State == TaskState.WaitingForRerunAfterError)
                {
                    meta.State = TaskState.Canceled;
                    handleTasksMetaStorage.AddMeta(meta);
                    return true;
                }
                return false;
            }
        }

        public bool RerunTask(string id, TimeSpan delay)
        {
            return false;
        }

        public RemoteTaskInfo GetTaskInfo(string taskId)
        {
            Task task = handleTaskCollection.GetTask(taskId);
            var res = (ITaskData)serializer.Deserialize(typeToNameMapper.GetTaskType(task.Meta.Name), task.Data);
            return new RemoteTaskInfo
                {
                    Context = task.Meta,
                    TaskData = res
                };
        }

        public RemoteTaskInfo<T> GetTaskInfo<T>(string taskId)
            where T : ITaskData
        {
            Task task = handleTaskCollection.GetTask(taskId);
            Type taskType = typeToNameMapper.GetTaskType(task.Meta.Name);
            if(!typeof(T).IsAssignableFrom(taskType))
                throw new Exception(string.Format("Type '{0}' is not assignable from '{1}'", typeof(T).FullName, taskType.FullName));
            var res = (T)serializer.Deserialize(taskType, task.Data);
            return new RemoteTaskInfo<T>
                {
                    Context = task.Meta,
                    TaskData = res
                };
        }

        public IRemoteTask CreateTask<T>(T taskData, string parentTaskId) where T : ITaskData
        {
            long nowTicks = DateTime.UtcNow.Ticks;
            string taskId = Guid.NewGuid().ToString();
            Type type = taskData.GetType();
            var task = new Task
                {
                    Data = serializer.Serialize(type, taskData),
                    Meta = new TaskMetaInformation
                        {
                            Attempts = 0,
                            Id = taskId,
                            Ticks = nowTicks,
                            Name = typeToNameMapper.GetTaskName(type),
                            ParentTaskId = parentTaskId,
                            State = TaskState.New,
                        }
                };
            return new RemoteTask(handleTaskCollection, task);
        }

        public IRemoteTask CreateTask<T>(T taskData) where T : ITaskData
        {
            return CreateTask(taskData, null);
        }

        public string Queue<T>(T taskData, TimeSpan delay, string parentTaskId) where T : ITaskData
        {
            IRemoteTask remoteTask = CreateTask(taskData, parentTaskId);
            remoteTask.Queue(delay);
            return remoteTask.Id;
        }

        public string Queue<T>(T taskData) where T : ITaskData
        {
            return Queue(taskData, TimeSpan.FromMilliseconds(0), null);
        }

        public string Queue<T>(T taskData, string parentTaskId) where T : ITaskData
        {
            return Queue(taskData, TimeSpan.FromMilliseconds(0), parentTaskId);
        }

        public string Queue<T>(T taskData, TimeSpan delay) where T : ITaskData
        {
            return Queue(taskData, delay, null);
        }

        private readonly IHandleTaskCollection handleTaskCollection;
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly ISerializer serializer;
        private readonly ITaskDataTypeToNameMapper typeToNameMapper;
        private readonly IRemoteLockCreator remoteLockCreator;
    }
}