using GroBuf;

using log4net;

using RemoteQueue.Handling;
using RemoteQueue.LocalTasks.Scheduling;
using RemoteQueue.LocalTasks.TaskQueue;
using RemoteQueue.Profiling;
using RemoteQueue.Settings;
using RemoteQueue.UserClasses;

using SKBKontur.Cassandra.CassandraClient.Clusters;

namespace RemoteQueue.Configuration
{
    public class ExchangeSchedulableRunner : IExchangeSchedulableRunner
    {
        public ExchangeSchedulableRunner(
            IExchangeSchedulableRunnerSettings runnerSettings,
            TaskHandlerRegistryBase taskHandlerRegistry,
            ISerializer serializer,
            ICassandraCluster cassandraCluster,
            ICassandraSettings cassandraSettings,
            IRemoteTaskQueueSettings taskQueueSettings,
            ITaskDataTypeToNameMapper taskDataTypeToNameMapper,
            IRemoteTaskQueueProfiler remoteTaskQueueProfiler)
        {
            this.runnerSettings = runnerSettings;
            var taskCounter = new TaskCounter(runnerSettings);
            var taskHandlerCollection = new TaskHandlerCollection(taskDataTypeToNameMapper, taskHandlerRegistry);
            var remoteTaskQueue = new RemoteTaskQueue(serializer, cassandraCluster, cassandraSettings, taskQueueSettings, taskDataTypeToNameMapper, remoteTaskQueueProfiler);
            var localTaskQueue = new LocalTaskQueue(taskCounter, taskHandlerCollection, remoteTaskQueue);
            handlerManager = new HandlerManager(taskDataTypeToNameMapper, localTaskQueue, remoteTaskQueue.HandleTasksMetaStorage, remoteTaskQueue.GlobalTime);
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

        private volatile bool worked;
        private readonly IExchangeSchedulableRunnerSettings runnerSettings;
        private readonly IHandlerManager handlerManager;
        private readonly object lockObject = new object();
        private readonly IPeriodicTaskRunner periodicTaskRunner = new PeriodicTaskRunner();
        private static readonly ILog logger = LogManager.GetLogger(typeof(ExchangeSchedulableRunner));
    }
}