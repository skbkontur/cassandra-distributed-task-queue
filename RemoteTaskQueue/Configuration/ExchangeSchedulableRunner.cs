using System;

using GroBuf;

using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using RemoteQueue.Handling;
using RemoteQueue.LocalTasks.Scheduling;
using RemoteQueue.LocalTasks.TaskQueue;
using RemoteQueue.Settings;
using RemoteQueue.UserClasses;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock;
using SKBKontur.Catalogue.CassandraPrimitives.Storages.Primitives;

using log4net;

namespace RemoteQueue.Configuration
{
    public class ExchangeSchedulableRunner : IExchangeSchedulableRunner
    {
        public ExchangeSchedulableRunner(ICassandraSettings cassandraSettings, IExchangeSchedulableRunnerSettings runnerSettings, TaskDataRegistryBase taskDataRegistry, TaskHandlerRegistryBase taskHandlerRegistry, ISerializer serializer)
        {
            this.runnerSettings = runnerSettings;
            periodicTaskRunner = new PeriodicTaskRunner();
            var cassandraCluster = new CassandraCluster(cassandraSettings);
            var parameters = new ColumnFamilyRepositoryParameters(cassandraCluster, cassandraSettings);
            var ticksHolder = new TicksHolder(serializer, parameters);
            var globalTime = new GlobalTime(ticksHolder);
            var taskMinimalStartTicksIndex = new TaskMinimalStartTicksIndex(parameters, ticksHolder, serializer, globalTime, cassandraSettings);
            var taskMetaInformationBlobStorage = new TaskMetaInformationBlobStorage(parameters, serializer, globalTime);
            var eventLongRepository = new EventLogRepository(serializer, globalTime, parameters, ticksHolder);
            var handleTasksMetaStorage = new HandleTasksMetaStorage(taskMetaInformationBlobStorage, taskMinimalStartTicksIndex, eventLongRepository, globalTime);
            var handleTaskCollection = new HandleTaskCollection(handleTasksMetaStorage, new TaskDataBlobStorage(parameters, serializer, globalTime));
            var handleTaskExceptionInfoStorage = new HandleTaskExceptionInfoStorage(new TaskExceptionInfoBlobStorage(parameters, serializer, globalTime));
            var remoteLockCreator = new RemoteLockCreator(new CassandraRemoteLockImplementation(cassandraCluster, parameters.Settings, serializer, new ColumnFamilyFullName(parameters.Settings.QueueKeyspace, parameters.LockColumnFamilyName)));
            var taskHandlerCollection = new TaskHandlerCollection(new TaskDataTypeToNameMapper(taskDataRegistry), taskHandlerRegistry);
            var remoteTaskQueue = new RemoteTaskQueue(globalTime, serializer, handleTasksMetaStorage, handleTaskCollection, remoteLockCreator, handleTaskExceptionInfoStorage, taskDataRegistry);
            var taskCounter = new TaskCounter(runnerSettings);
            handlerManager = new HandlerManager(new TaskQueue(), taskCounter, new ShardingManager(runnerSettings), (taskInfo, startProcessingTicks) => new HandlerTask(taskInfo, startProcessingTicks, taskCounter, serializer, remoteTaskQueue, handleTaskCollection, remoteLockCreator, handleTaskExceptionInfoStorage, taskHandlerCollection, handleTasksMetaStorage, taskMinimalStartTicksIndex), handleTasksMetaStorage);
            handleTasksMetaStorage.OnIndexMeta = info => ((HandlerManager)handlerManager).QueueTask(info, DateTime.UtcNow.Ticks, TaskQueueReason.TaskContinuation);
            RemoteTaskQueue = remoteTaskQueue;
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

        public IRemoteTaskQueue RemoteTaskQueue { get; private set; }

        private readonly IExchangeSchedulableRunnerSettings runnerSettings;

        private readonly IHandlerManager handlerManager;
        private readonly object lockObject = new object();
        private readonly ILog logger = LogManager.GetLogger(typeof(ExchangeSchedulableRunner));

        private readonly IPeriodicTaskRunner periodicTaskRunner;
        private volatile bool worked;
    }
}