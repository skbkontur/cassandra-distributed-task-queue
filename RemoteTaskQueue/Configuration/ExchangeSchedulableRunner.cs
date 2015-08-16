using System;

using GroBuf;

using log4net;

using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Cassandra.Repositories.Indexes.ChildTaskIndex;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using RemoteQueue.Handling;
using RemoteQueue.LocalTasks.Scheduling;
using RemoteQueue.LocalTasks.TaskQueue;
using RemoteQueue.Profiling;
using RemoteQueue.Settings;
using RemoteQueue.UserClasses;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock;
using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock.RemoteLocker;
using SKBKontur.Catalogue.CassandraPrimitives.Storages.Primitives;

namespace RemoteQueue.Configuration
{
    public class ExchangeSchedulableRunner : IExchangeSchedulableRunner
    {
        public ExchangeSchedulableRunner(ICassandraCluster cassandraCluster, ICassandraSettings cassandraSettings, IRemoteTaskQueueSettings taskQueueSettings, IExchangeSchedulableRunnerSettings runnerSettings, TaskDataRegistryBase taskDataRegistry, TaskHandlerRegistryBase taskHandlerRegistry, ISerializer serializer, IRemoteTaskQueueProfiler remoteTaskQueueProfiler)
        {
            this.runnerSettings = runnerSettings;
            periodicTaskRunner = new PeriodicTaskRunner();
            var parameters = new ColumnFamilyRepositoryParameters(cassandraCluster, cassandraSettings);
            var ticksHolder = new TicksHolder(serializer, parameters);
            var globalTime = new GlobalTime(ticksHolder);
            var taskMinimalStartTicksIndex = new TaskMinimalStartTicksIndex(parameters, ticksHolder, serializer, globalTime);
            var taskMetaInformationBlobStorage = new TaskMetaInformationBlobStorage(parameters, serializer, globalTime);
            var eventLongRepository = new EventLogRepository(serializer, globalTime, parameters, ticksHolder);
            var childTaskIndex = new ChildTaskIndex(parameters, serializer, taskMetaInformationBlobStorage);
            var handleTasksMetaStorage = new HandleTasksMetaStorage(taskMetaInformationBlobStorage, taskMinimalStartTicksIndex, eventLongRepository, globalTime, childTaskIndex);
            var handleTaskCollection = new HandleTaskCollection(handleTasksMetaStorage, new TaskDataBlobStorage(parameters, serializer, globalTime), remoteTaskQueueProfiler);
            var handleTaskExceptionInfoStorage = new HandleTaskExceptionInfoStorage(new TaskExceptionInfoBlobStorage(parameters, serializer, globalTime));
            var remoteLockImplementationSettings = CassandraRemoteLockImplementationSettings.Default(new ColumnFamilyFullName(parameters.Settings.QueueKeyspace, parameters.LockColumnFamilyName));
            var remoteLockImplementation = new CassandraRemoteLockImplementation(cassandraCluster, serializer, remoteLockImplementationSettings);
            var remoteLockCreator = taskQueueSettings.UseRemoteLocker ? (IRemoteLockCreator)new RemoteLocker(remoteLockImplementation, new RemoteLockerMetrics(parameters.Settings.QueueKeyspace)) : new RemoteLockCreator(remoteLockImplementation);
            var taskHandlerCollection = new TaskHandlerCollection(new TaskDataTypeToNameMapper(taskDataRegistry), taskHandlerRegistry);
            var remoteTaskQueue = new RemoteTaskQueue(serializer, handleTasksMetaStorage, handleTaskCollection, remoteLockCreator, handleTaskExceptionInfoStorage, taskDataRegistry, childTaskIndex);
            var taskCounter = new TaskCounter(runnerSettings);
            var localTaskQueue = new LocalTaskQueue((taskId, reason, taskInfo, meta, startProcessingTicks) =>
                                          new HandlerTask(taskId, reason, taskInfo, meta, startProcessingTicks, taskCounter, serializer, remoteTaskQueue, handleTaskCollection, remoteLockCreator, handleTaskExceptionInfoStorage, taskHandlerCollection, handleTasksMetaStorage, taskMinimalStartTicksIndex, remoteTaskQueueProfiler));
            handlerManager = new HandlerManager(localTaskQueue, taskCounter, taskHandlerCollection, handleTasksMetaStorage);
        }

        public void Stop()
        {
            if(worked)
            {
                lock(lockObject)
                {
                    if(worked)
                    {
                        periodicTaskRunner.Unregister(handlerManager.Id, 15000);
                        handlerManager.Stop();
                        worked = false;
                        logger.Info("Stop ExchangeSchedulableRunner.");
                    }
                }
            }
        }

        public void Start()
        {
            if(!worked)
            {
                lock(lockObject)
                {
                    if(!worked)
                    {
                        handlerManager.Start();
                        periodicTaskRunner.Register(handlerManager, runnerSettings.PeriodicInterval);
                        worked = true;
                        logger.InfoFormat("Start ExchangeSchedulableRunner: schedule handlerManager with period {0}", runnerSettings.PeriodicInterval);
                    }
                }
            }
        }

        public Tuple<long, long> GetQueueLength()
        {
            return handlerManager.GetCassandraQueueLength();
        }

        private readonly IExchangeSchedulableRunnerSettings runnerSettings;
        private readonly IHandlerManager handlerManager;
        private readonly object lockObject = new object();
        private readonly ILog logger = LogManager.GetLogger(typeof(ExchangeSchedulableRunner));
        private readonly IPeriodicTaskRunner periodicTaskRunner;
        private volatile bool worked;
    }
}