using System;

using GroBuf;

using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.RemoteLock;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Cassandra.Repositories.Indexes.EventIndexes;
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
        public ExchangeSchedulableRunner(ICassandraSettings cassandraSettings, IExchangeSchedulableRunnerSettings runnerSettings, ITaskDataRegistry taskDataRegistry, ITaskHandlerRegistry taskHandlerRegistry)
        {
            this.runnerSettings = runnerSettings;
            ISerializer serializer = StaticGrobuf.GetSerializer();
            periodicTaskRunner = new PeriodicTaskRunner();
            var cassandraCluster = new CassandraCluster(cassandraSettings);
            var parameters = new ColumnFamilyRepositoryParameters(cassandraCluster, cassandraSettings);
            var ticksHolder = new TicksHolder(serializer, parameters);
            var globalTime = new GlobalTime(ticksHolder);
            var taskMetaEventColumnInfoIndex = new TaskMetaEventColumnInfoIndex(serializer, globalTime, parameters);
            var indexRecordsCleaner = new IndexRecordsCleaner(parameters, taskMetaEventColumnInfoIndex, serializer, globalTime);
            var taskMinimalStartTicksIndex = new TaskMinimalStartTicksIndex(parameters, taskMetaEventColumnInfoIndex, indexRecordsCleaner, ticksHolder, serializer, globalTime, cassandraSettings);
            var taskMetaInformationBlobStorage = new TaskMetaInformationBlobStorage(parameters, serializer, globalTime);
            var eventLongRepository = new EventLogRepository(serializer, globalTime, parameters, ticksHolder);
            var handleTasksMetaStorage = new HandleTasksMetaStorage(taskMetaInformationBlobStorage, taskMinimalStartTicksIndex, eventLongRepository);
            var handleTaskCollection = new HandleTaskCollection(handleTasksMetaStorage, new TaskDataBlobStorage(parameters, serializer, globalTime));
            var handleTaskExceptionInfoStorage = new HandleTaskExceptionInfoStorage(new TaskExceptionInfoBlobStorage(parameters, serializer, globalTime));
            var remoteLockCreator = new RemoteLockCreator(new LockRepository(parameters));
            var taskHandlerCollection = new TaskHandlerCollection(new TaskDataTypeToNameMapper(taskDataRegistry), taskHandlerRegistry);
            var remoteTaskQueue = new RemoteTaskQueue(cassandraSettings, taskDataRegistry);
            var taskCounter = new TaskCounter(runnerSettings);
            handlerManager = new HandlerManager(new TaskQueue(), taskCounter, new ShardingManager(runnerSettings), id => new HandlerTask(id, taskCounter, serializer, remoteTaskQueue, handleTaskCollection, remoteLockCreator, handleTaskExceptionInfoStorage, taskHandlerCollection, handleTasksMetaStorage, indexRecordsCleaner), handleTasksMetaStorage);
        }

        //public ExchangeSchedulableRunner(IPeriodicTaskRunner periodicTaskRunner, IHandlerManager handlerManager)
        //{
        //    this.periodicTaskRunner = periodicTaskRunner;
        //    this.handlerManager = handlerManager;
        //}

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