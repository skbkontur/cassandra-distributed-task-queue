using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GroBuf;

using RemoteQueue.Handling;
using RemoteQueue.LocalTasks.TaskQueue;
using RemoteQueue.Profiling;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.Core.Graphite.Client.Relay;
using SKBKontur.Catalogue.Core.Graphite.Client.Settings;
using SKBKontur.Catalogue.ServiceLib.Logging;
using SKBKontur.Catalogue.ServiceLib.Scheduling;

namespace RemoteQueue.Configuration
{
    public class ExchangeSchedulableRunner : IExchangeSchedulableRunner, IDisposable
    {
        public ExchangeSchedulableRunner(
            IExchangeSchedulableRunnerSettings runnerSettings,
            IPeriodicTaskRunner periodicTaskRunner,
            ICatalogueGraphiteClient graphiteClient,
            IProjectWideGraphitePathPrefixProvider graphitePathPrefixProvider,
            ITaskDataRegistry taskDataRegistry,
            ITaskHandlerRegistry taskHandlerRegistry,
            ISerializer serializer,
            ICassandraCluster cassandraCluster,
            ICassandraSettings cassandraSettings,
            IRemoteTaskQueueSettings taskQueueSettings,
            IRemoteTaskQueueProfiler remoteTaskQueueProfiler)
        {
            this.runnerSettings = runnerSettings;
            this.periodicTaskRunner = periodicTaskRunner;
            var taskCounter = new TaskCounter(runnerSettings.MaxRunningTasksCount, runnerSettings.MaxRunningContinuationsCount);
            var remoteTaskQueue = new RemoteTaskQueue(serializer, cassandraCluster, cassandraSettings, taskQueueSettings, taskDataRegistry, remoteTaskQueueProfiler);
            localTaskQueue = new LocalTaskQueue(taskCounter, taskHandlerRegistry, remoteTaskQueue);
            handlerManagers.Add(new HandlerManager(string.Empty, runnerSettings.MaxRunningTasksCount, localTaskQueue, remoteTaskQueue.HandleTasksMetaStorage, remoteTaskQueue.GlobalTime));
            foreach(var taskTopic in taskHandlerRegistry.GetAllTaskTopicsToHandle())
                handlerManagers.Add(new HandlerManager(taskTopic, runnerSettings.MaxRunningTasksCount, localTaskQueue, remoteTaskQueue.HandleTasksMetaStorage, remoteTaskQueue.GlobalTime));
            reportConsumerStateToGraphiteTask = new ReportConsumerStateToGraphiteTask(graphiteClient, graphitePathPrefixProvider, handlerManagers);
        }

        public void Dispose()
        {
            Stop();
        }

        public void Start()
        {
            if(!started)
            {
                lock(lockObject)
                {
                    if(!started)
                    {
                        localTaskQueue.Start();
                        foreach(var handlerManager in handlerManagers)
                            periodicTaskRunner.Register(handlerManager, runnerSettings.PeriodicInterval);
                        periodicTaskRunner.Register(reportConsumerStateToGraphiteTask, TimeSpan.FromMinutes(1));
                        started = true;
                        Log.For(this).InfoFormat("Start ExchangeSchedulableRunner: schedule handlerManagers[{0}] with period {1}:\r\n{2}", handlerManagers.Count, runnerSettings.PeriodicInterval, string.Join("\r\n", handlerManagers.Select(x => x.Id)));
                    }
                }
            }
        }

        public void Stop()
        {
            if(started)
            {
                lock(lockObject)
                {
                    if(started)
                    {
                        periodicTaskRunner.Unregister(reportConsumerStateToGraphiteTask.Id, 15000);
                        Task.WaitAll(handlerManagers.Select(theHandlerManager => Task.Factory.StartNew(() => { periodicTaskRunner.Unregister(theHandlerManager.Id, 15000); })).ToArray());
                        localTaskQueue.StopAndWait(TimeSpan.FromSeconds(100));
                        started = false;
                        Log.For(this).Info("Stop ExchangeSchedulableRunner.");
                    }
                }
            }
        }

        private volatile bool started;
        private readonly IExchangeSchedulableRunnerSettings runnerSettings;
        private readonly IPeriodicTaskRunner periodicTaskRunner;
        private readonly ReportConsumerStateToGraphiteTask reportConsumerStateToGraphiteTask;
        private readonly object lockObject = new object();
        private readonly LocalTaskQueue localTaskQueue;
        private readonly List<IHandlerManager> handlerManagers = new List<IHandlerManager>();
    }
}