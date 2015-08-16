using System;

using GroBuf;

using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Cassandra.Repositories.Indexes.ChildTaskIndex;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using RemoteQueue.Profiling;
using RemoteQueue.Settings;
using RemoteQueue.UserClasses;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock;
using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock.RemoteLocker;
using SKBKontur.Catalogue.CassandraPrimitives.Storages.Primitives;

namespace RemoteQueue.Handling
{
    public class RemoteTaskQueue : IRemoteTaskQueue
    {
        public RemoteTaskQueue(
            ISerializer serializer,
            ICassandraCluster cassandraCluster,
            ICassandraSettings settings,
            IRemoteTaskQueueSettings taskQueueSettings,
            TaskDataRegistryBase taskDataRegistry,
            IRemoteTaskQueueProfiler remoteTaskQueueProfiler)
        {
            var parameters = new ColumnFamilyRepositoryParameters(cassandraCluster, settings);
            var ticksHolder = new TicksHolder(serializer, parameters);
            var globalTime = new GlobalTime(ticksHolder);
            var taskMinimalStartTicksIndex = new TaskMinimalStartTicksIndex(parameters, ticksHolder, serializer, globalTime);
            var taskMetaInformationBlobStorage = new TaskMetaInformationBlobStorage(parameters, serializer, globalTime);
            var eventLongRepository = new EventLogRepository(serializer, globalTime, parameters, ticksHolder);
            var childTaskIndex = new ChildTaskIndex(parameters, serializer, taskMetaInformationBlobStorage);
            var handleTasksMetaStorage = new HandleTasksMetaStorage(taskMetaInformationBlobStorage, taskMinimalStartTicksIndex, eventLongRepository, globalTime, childTaskIndex);
            handleTaskCollection = new HandleTaskCollection(handleTasksMetaStorage, new TaskDataBlobStorage(parameters, serializer, globalTime), remoteTaskQueueProfiler);
            var remoteLockImplementationSettings = CassandraRemoteLockImplementationSettings.Default(new ColumnFamilyFullName(parameters.Settings.QueueKeyspace, parameters.LockColumnFamilyName));
            var remoteLockImplementation = new CassandraRemoteLockImplementation(cassandraCluster, serializer, remoteLockImplementationSettings);
            var remoteLockCreator = taskQueueSettings.UseRemoteLocker ? (IRemoteLockCreator)new RemoteLocker(remoteLockImplementation, new RemoteLockerMetrics(parameters.Settings.QueueKeyspace)) : new RemoteLockCreator(remoteLockImplementation);
            var handleTaskExceptionInfoStorage = new HandleTaskExceptionInfoStorage(new TaskExceptionInfoBlobStorage(parameters, serializer, globalTime));
            remoteTaskQueueImpl = new RemoteTaskQueueImpl(serializer, handleTasksMetaStorage, handleTaskCollection, remoteLockCreator, handleTaskExceptionInfoStorage, taskDataRegistry, childTaskIndex);
        }

        public bool CancelTask(string taskId)
        {
            return remoteTaskQueueImpl.CancelTask(taskId);
        }

        public bool RerunTask(string taskId, TimeSpan delay)
        {
            return remoteTaskQueueImpl.RerunTask(taskId, delay);
        }

        public RemoteTaskInfo GetTaskInfo(string taskId)
        {
            return remoteTaskQueueImpl.GetTaskInfo(taskId);
        }

        public RemoteTaskInfo<T> GetTaskInfo<T>(string taskId)
            where T : ITaskData
        {
            return remoteTaskQueueImpl.GetTaskInfo<T>(taskId);
        }

        public RemoteTaskInfo[] GetTaskInfos(string[] taskIds)
        {
            return remoteTaskQueueImpl.GetTaskInfos(taskIds);
        }

        public RemoteTaskInfo<T>[] GetTaskInfos<T>(string[] taskIds) where T : ITaskData
        {
            return remoteTaskQueueImpl.GetTaskInfos<T>(taskIds);
        }

        public IRemoteTask CreateTask<T>(T taskData, CreateTaskOptions createTaskOptions) where T : ITaskData
        {
            var task = remoteTaskQueueImpl.CreateTaskImpl(taskData, createTaskOptions);
            return new RemoteTask(task, handleTaskCollection);
        }

        public string[] GetChildrenTaskIds(string taskId)
        {
            return remoteTaskQueueImpl.GetChildrenTaskIds(taskId);
        }

        private readonly IHandleTaskCollection handleTaskCollection;
        private readonly RemoteTaskQueueImpl remoteTaskQueueImpl;
    }
}