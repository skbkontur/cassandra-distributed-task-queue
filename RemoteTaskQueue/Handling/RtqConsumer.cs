using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.LocalTasks.TaskQueue;

using SKBKontur.Catalogue.ServiceLib.Scheduling;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
{
    [PublicAPI]
    public class RtqConsumer : IDisposable, IRtqConsumer
    {
        public RtqConsumer(IRtqConsumerSettings consumerSettings,
                           IPeriodicTaskRunner periodicTaskRunner,
                           IRtqTaskHandlerRegistry taskHandlerRegistry,
                           RemoteTaskQueue remoteTaskQueue)
        {
            this.consumerSettings = consumerSettings;
            this.periodicTaskRunner = periodicTaskRunner;
            RtqInternals = remoteTaskQueue;
            var localQueueTaskCounter = new LocalQueueTaskCounter(consumerSettings.MaxRunningTasksCount, consumerSettings.MaxRunningContinuationsCount);
            localTaskQueue = new LocalTaskQueue(localQueueTaskCounter, taskHandlerRegistry, remoteTaskQueue);
            foreach (var taskTopic in taskHandlerRegistry.GetAllTaskTopicsToHandle())
                handlerManagers.Add(new HandlerManager(remoteTaskQueue.QueueKeyspace, taskTopic, consumerSettings.MaxRunningTasksCount, localTaskQueue, remoteTaskQueue.HandleTasksMetaStorage, remoteTaskQueue.GlobalTime, remoteTaskQueue.Logger));
            reportConsumerStateToGraphiteTask = new ReportConsumerStateToGraphiteTask(remoteTaskQueue.QueueKeyspace, remoteTaskQueue.Profiler, handlerManagers);
            logger = remoteTaskQueue.Logger.ForContext(nameof(RtqConsumer));
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
                        RtqInternals.ResetTicksHolderInMemoryState();
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
                        RtqInternals.ResetTicksHolderInMemoryState();
                        started = false;
                        logger.Info("RtqConsumer stopped");
                    }
                }
            }
        }

        [NotNull]
        internal IRtqInternals RtqInternals { get; }

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