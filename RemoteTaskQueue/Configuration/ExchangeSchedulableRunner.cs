using System;

using GroBuf;

using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.RemoteLock;
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

using log4net;

namespace RemoteQueue.Configuration
{
    public class ExchangeSchedulableRunner : IExchangeSchedulableRunner
    {
        public ExchangeSchedulableRunner(ICassandraClusterSettings cassandraSettings, IRemoteTaskQueueCassandraSettings remoteTaskQueueCassandraSettings, IExchangeSchedulableRunnerSettings runnerSettings, TaskDataRegistryBase taskDataRegistry, TaskHandlerRegistryBase taskHandlerRegistry)
        {
            this.runnerSettings = runnerSettings;
            ISerializer serializer = StaticGrobuf.GetSerializer();
            periodicTaskRunner = new PeriodicTaskRunner();
            var cassandraCluster = new CassandraCluster(cassandraSettings);
            var parameters = new ColumnFamilyRepositoryParameters(cassandraCluster, cassandraSettings, remoteTaskQueueCassandraSettings);
            var ticksHolder = new TicksHolder(serializer, parameters);
            var globalTime = new GlobalTime(ticksHolder);
            var taskMinimalStartTicksIndex = new TaskMinimalStartTicksIndex(parameters, ticksHolder, serializer, globalTime, cassandraSettings);
            var taskMetaInformationBlobStorage = new TaskMetaInformationBlobStorage(parameters, serializer, globalTime);
            var eventLongRepository = new EventLogRepository(serializer, globalTime, parameters, ticksHolder);
            var handleTasksMetaStorage = new HandleTasksMetaStorage(taskMetaInformationBlobStorage, taskMinimalStartTicksIndex, eventLongRepository, globalTime);
            var handleTaskCollection = new HandleTaskCollection(handleTasksMetaStorage, new TaskDataBlobStorage(parameters, serializer, globalTime));
            var handleTaskExceptionInfoStorage = new HandleTaskExceptionInfoStorage(new TaskExceptionInfoBlobStorage(parameters, serializer, globalTime));
            var remoteLockCreator = new RemoteLockCreator(new LockRepository(parameters));
            var taskHandlerCollection = new TaskHandlerCollection(new TaskDataTypeToNameMapper(taskDataRegistry), taskHandlerRegistry);
            var remoteTaskQueue = new RemoteTaskQueue(cassandraSettings, remoteTaskQueueCassandraSettings, taskDataRegistry);
            var taskCounter = new TaskCounter(runnerSettings);
            remoteTaskQueueHandlerManager = new RemoteTaskQueueHandlerManager(new TaskQueue(), taskCounter, new ShardingManager(runnerSettings), taskInfo => new HandlerTask(taskInfo, taskCounter, serializer, remoteTaskQueue, handleTaskCollection, remoteLockCreator, handleTaskExceptionInfoStorage, taskHandlerCollection, handleTasksMetaStorage, taskMinimalStartTicksIndex), handleTasksMetaStorage, remoteTaskQueue);
        }

        public void Stop()
        {
            if(worked)
            {
                lock(lockObject)
                {
                    if(worked)
                    {
                        periodicTaskRunner.Unregister(remoteTaskQueueHandlerManager.Id, 15000);
                        remoteTaskQueueHandlerManager.Stop();
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
                        remoteTaskQueueHandlerManager.Start();
                        periodicTaskRunner.Register(remoteTaskQueueHandlerManager, runnerSettings.PeriodicInterval);
                        worked = true;
                        logger.InfoFormat("Start ExchangeSchedulableRunner: schedule handlerManager with period {0}", runnerSettings.PeriodicInterval);
                    }
                }
            }
        }

        public Tuple<long, long> GetCassandraQueueLength()
        {
            return remoteTaskQueueHandlerManager.GetCassandraQueueLength();
        }

        public long GetQueueLength()
        {
            return remoteTaskQueueHandlerManager.GetQueueLength();
        }

        public void CancelAllTasks()
        {
            remoteTaskQueueHandlerManager.CancelAllTasks();
        }

        private readonly IExchangeSchedulableRunnerSettings runnerSettings;

        private readonly IRemoteTaskQueueHandlerManager remoteTaskQueueHandlerManager;
        private readonly object lockObject = new object();
        private readonly ILog logger = LogManager.GetLogger(typeof(ExchangeSchedulableRunner));

        private readonly IPeriodicTaskRunner periodicTaskRunner;
        private volatile bool worked;
    }
}