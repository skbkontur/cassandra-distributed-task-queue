using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            var taskCounter = new TaskCounter(runnerSettings.MaxRunningTasksCount, runnerSettings.MaxRunningContinuationsCount);
            var taskHandlerCollection = new TaskHandlerCollection(taskDataTypeToNameMapper, taskHandlerRegistry);
            var remoteTaskQueue = new RemoteTaskQueue(serializer, cassandraCluster, cassandraSettings, taskQueueSettings, taskDataTypeToNameMapper, remoteTaskQueueProfiler);
            var localTaskQueue = new LocalTaskQueue(taskCounter, taskHandlerCollection, remoteTaskQueue);
            handlerManagers.Add(new HandlerManager(null, runnerSettings.MaxRunningTasksCount, localTaskQueue, remoteTaskQueue.HandleTasksMetaStorage, remoteTaskQueue.GlobalTime));
            foreach(var taskTopic in taskDataTypeToNameMapper.GetAllTaskNames())
                handlerManagers.Add(new HandlerManager(taskTopic, runnerSettings.MaxRunningTasksCount, localTaskQueue, remoteTaskQueue.HandleTasksMetaStorage, remoteTaskQueue.GlobalTime));
        }

        public void Stop()
        {
            if(worked)
            {
                lock(lockObject)
                {
                    if(worked)
                    {
                        Task.WaitAll(handlerManagers.Select(theHandlerManager => Task.Factory.StartNew(() =>
                            {
                                periodicTaskRunner.Unregister(theHandlerManager.Id, 15000);
                                theHandlerManager.Stop();
                            })).ToArray());
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
                        foreach(var handlerManager in handlerManagers)
                        {
                            handlerManager.Start();
                            periodicTaskRunner.Register(handlerManager, runnerSettings.PeriodicInterval);
                        }
                        worked = true;
                        logger.InfoFormat("Start ExchangeSchedulableRunner: schedule handlerManagers[{0}] with period {1}:\r\n{2}", handlerManagers.Count, runnerSettings.PeriodicInterval, string.Join("\r\n", handlerManagers.Select(x => x.Id)));
                    }
                }
            }
        }

        private volatile bool worked;
        private readonly IExchangeSchedulableRunnerSettings runnerSettings;
        private readonly List<IHandlerManager> handlerManagers = new List<IHandlerManager>();
        private readonly object lockObject = new object();
        private readonly IPeriodicTaskRunner periodicTaskRunner = new PeriodicTaskRunner();
        private static readonly ILog logger = LogManager.GetLogger(typeof(ExchangeSchedulableRunner));
    }
}