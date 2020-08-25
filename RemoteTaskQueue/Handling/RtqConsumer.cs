using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.LocalTasks.TaskQueue;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
{
    [PublicAPI]
    public class RtqConsumer : IDisposable, IRtqConsumer
    {
        public RtqConsumer(IRtqConsumerSettings consumerSettings,
                           IRtqPeriodicJobRunner periodicJobRunner,
                           IRtqTaskHandlerRegistry taskHandlerRegistry,
                           RemoteTaskQueue remoteTaskQueue)
        {
            this.consumerSettings = consumerSettings;
            this.periodicJobRunner = periodicJobRunner;
            RtqInternals = remoteTaskQueue;
            var localQueueTaskCounter = new LocalQueueTaskCounter(consumerSettings.MaxRunningTasksCount, consumerSettings.MaxRunningContinuationsCount);
            localTaskQueue = new LocalTaskQueue(localQueueTaskCounter, taskHandlerRegistry, remoteTaskQueue);
            foreach (var taskTopic in taskHandlerRegistry.GetAllTaskTopicsToHandle())
            {
                var handlerManager = new HandlerManager(remoteTaskQueue.QueueKeyspace,
                                                        taskTopic,
                                                        consumerSettings.MaxRunningTasksCount,
                                                        localTaskQueue,
                                                        remoteTaskQueue.HandleTasksMetaStorage,
                                                        remoteTaskQueue.GlobalTime,
                                                        remoteTaskQueue.Logger);
                handlerManagers.Add(handlerManager);
            }
            reportConsumerStateToGraphitePeriodicJob = new ReportConsumerStateToGraphitePeriodicJob(remoteTaskQueue.QueueKeyspace, remoteTaskQueue.Profiler, handlerManagers);
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
                        {
                            periodicJobRunner.RunPeriodicJob(handlerManager.JobId,
                                                             delayBetweenIterations : consumerSettings.PeriodicInterval,
                                                             handlerManager.RunJobIteration);
                        }
                        periodicJobRunner.RunPeriodicJob(reportConsumerStateToGraphitePeriodicJob.JobId,
                                                         delayBetweenIterations : TimeSpan.FromMinutes(1),
                                                         reportConsumerStateToGraphitePeriodicJob.RunJobIteration);
                        started = true;
                        var handlerManagerIds = string.Join("\r\n", handlerManagers.Select(x => x.JobId));
                        logger.Info("Start RtqConsumer: schedule handlerManagers[{HandlerManagersCount}] with period {Period}:\r\n{HandlerManagerIds}",
                                    new {HandlerManagersCount = handlerManagers.Count, Period = consumerSettings.PeriodicInterval, HandlerManagerIds = handlerManagerIds});
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
                        periodicJobRunner.StopPeriodicJob(reportConsumerStateToGraphitePeriodicJob.JobId);
                        Task.WaitAll(handlerManagers.Select(theHandlerManager => Task.Factory.StartNew(() =>
                            {
                                // comment to prevent ugly reformat
                                periodicJobRunner.StopPeriodicJob(theHandlerManager.JobId);
                            })).ToArray());
                        localTaskQueue.StopAndWait(timeout : TimeSpan.FromSeconds(100));
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
        private readonly IRtqPeriodicJobRunner periodicJobRunner;
        private readonly LocalTaskQueue localTaskQueue;
        private readonly ReportConsumerStateToGraphitePeriodicJob reportConsumerStateToGraphitePeriodicJob;
        private readonly ILog logger;
        private readonly object lockObject = new object();
        private readonly List<IHandlerManager> handlerManagers = new List<IHandlerManager>();
    }
}