using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GroBuf;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.DistributedTaskQueue.LocalTasks.TaskQueue;
using SkbKontur.Cassandra.DistributedTaskQueue.Profiling;
using SkbKontur.Cassandra.DistributedTaskQueue.Settings;
using SkbKontur.Cassandra.GlobalTimestamp;
using SkbKontur.Cassandra.ThriftClient.Clusters;

using SKBKontur.Catalogue.ServiceLib.Scheduling;

using Vostok.Logging.Abstractions;

#pragma warning disable 618

namespace SkbKontur.Cassandra.DistributedTaskQueue.Configuration
{
    public class RtqConsumer : IDisposable
    {
        public RtqConsumer(ILog logger,
                           IRtqConsumerSettings consumerSettings,
                           IPeriodicTaskRunner periodicTaskRunner,
                           IRtqTaskDataRegistry taskDataRegistry,
                           IRtqTaskHandlerRegistry taskHandlerRegistry,
                           ISerializer serializer,
                           IGlobalTime globalTime,
                           ICassandraCluster cassandraCluster,
                           IRtqSettings rtqSettings,
                           IRtqProfiler rtqProfiler)
        {
            this.consumerSettings = consumerSettings;
            this.periodicTaskRunner = periodicTaskRunner;
            var localQueueTaskCounter = new LocalQueueTaskCounter(consumerSettings.MaxRunningTasksCount, consumerSettings.MaxRunningContinuationsCount);
            var remoteTaskQueue = new RemoteTaskQueue(logger, serializer, globalTime, cassandraCluster, rtqSettings, taskDataRegistry, rtqProfiler);
            localTaskQueue = new LocalTaskQueue(localQueueTaskCounter, taskHandlerRegistry, remoteTaskQueue);
            foreach (var taskTopic in taskHandlerRegistry.GetAllTaskTopicsToHandle())
                handlerManagers.Add(new HandlerManager(taskTopic, consumerSettings.MaxRunningTasksCount, localTaskQueue, remoteTaskQueue.HandleTasksMetaStorage, remoteTaskQueue.GlobalTime, remoteTaskQueue.Logger));
            reportConsumerStateToGraphiteTask = new ReportConsumerStateToGraphiteTask(rtqProfiler, handlerManagers);
            RtqBackdoor = remoteTaskQueue;
            this.logger = remoteTaskQueue.Logger.ForContext(nameof(RtqConsumer));
        }

        public void Dispose()
        {
            Stop();
        }

        public void Start()
        {
            if (!started)
            {
                lock (lockObject)
                {
                    if (!started)
                    {
                        RtqBackdoor.ResetTicksHolderInMemoryState();
                        localTaskQueue.Start();
                        foreach (var handlerManager in handlerManagers)
                            periodicTaskRunner.Register(handlerManager, consumerSettings.PeriodicInterval);
                        periodicTaskRunner.Register(reportConsumerStateToGraphiteTask, TimeSpan.FromMinutes(1));
                        started = true;
                        var handlerManagerIds = string.Join("\r\n", handlerManagers.Select(x => x.Id));
                        logger.Info($"Start RtqConsumer: schedule handlerManagers[{handlerManagers.Count}] with period {consumerSettings.PeriodicInterval}:\r\n{handlerManagerIds}");
                    }
                }
            }
        }

        public void Stop()
        {
            if (started)
            {
                lock (lockObject)
                {
                    if (started)
                    {
                        logger.Info("Stopping RtqConsumer");
                        periodicTaskRunner.Unregister(reportConsumerStateToGraphiteTask.Id, 15000);
                        Task.WaitAll(handlerManagers.Select(theHandlerManager => Task.Factory.StartNew(() => { periodicTaskRunner.Unregister(theHandlerManager.Id, 15000); })).ToArray());
                        localTaskQueue.StopAndWait(TimeSpan.FromSeconds(100));
                        RtqBackdoor.ResetTicksHolderInMemoryState();
                        started = false;
                        logger.Info("RtqConsumer stopped");
                    }
                }
            }
        }

        [Obsolete("Only for usage in tests")]
        internal IRtqBackdoor RtqBackdoor { get; }

        private volatile bool started;
        private readonly IRtqConsumerSettings consumerSettings;
        private readonly IPeriodicTaskRunner periodicTaskRunner;
        private readonly LocalTaskQueue localTaskQueue;
        private readonly ReportConsumerStateToGraphiteTask reportConsumerStateToGraphiteTask;
        private readonly ILog logger;
        private readonly object lockObject = new object();
        private readonly List<IHandlerManager> handlerManagers = new List<IHandlerManager>();
    }
}