using System;
using System.Diagnostics;
using System.Threading;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedLock;
using SkbKontur.Cassandra.DistributedTaskQueue.Scheduling;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.EventFeeds
{
    public class PeriodicJobWithLeaderElection
    {
        public PeriodicJobWithLeaderElection(
            [NotNull] IRemoteLockCreator remoteLockCreator,
            [NotNull] string jobName,
            TimeSpan delayBetweenIterations,
            [NotNull] Action jobAction,
            [CanBeNull] Action onTakeTheLead,
            [CanBeNull] Action onLoseTheLead,
            [NotNull] ILog logger)
        {
            this.remoteLockCreator = remoteLockCreator;
            this.jobName = jobName;
            this.delayBetweenIterations = delayBetweenIterations;
            this.jobAction = jobAction;
            this.onTakeTheLead = onTakeTheLead;
            this.onLoseTheLead = onLoseTheLead;
            this.logger = logger;
            stopSignal = new ManualResetEventSlim(false);
            jobThread = new Thread(ThreadProc)
                {
                    Name = jobName,
                    IsBackground = true,
                };
            jobThread.Start();
            this.logger.Info($"Job thread has started for: {jobName}");
        }

        public void Stop()
        {
            stopSignal.Set();
            jobThread.Join();
            logger.Info($"Job thread has stopped for: {jobName}");
        }

        private void ThreadProc()
        {
            do
            {
                try
                {
                    var lockId = $"LeaderElection/{jobName}";
                    if (!remoteLockCreator.TryGetLock(lockId, out var leaderLock))
                        continue;
                    logger.Info("Leadership acquired for: {JobName}", new {JobName = jobName});
                    using (leaderLock)
                    {
                        onTakeTheLead?.Invoke();
                        try
                        {
                            LeaderThreadProc();
                        }
                        finally
                        {
                            onLoseTheLead?.Invoke();
                        }
                    }
                    logger.Info("Leadership released for: {JobName}", new {JobName = jobName});
                }
                catch (Exception e)
                {
                    logger.Error(e, "Leadership lost with unhandled exception on job thread for: {JobName}", new {JobName = jobName});
                }
            } while (!stopSignal.Wait(delayBetweenIterations));
        }

        private void LeaderThreadProc()
        {
            Stopwatch iterationStopwatch;
            do
            {
                iterationStopwatch = Stopwatch.StartNew();
                jobAction();
            } while (!stopSignal.Wait(DateTimeMath.Max(TimeSpan.Zero, delayBetweenIterations - iterationStopwatch.Elapsed)));
        }

        private readonly IRemoteLockCreator remoteLockCreator;
        private readonly string jobName;
        private readonly TimeSpan delayBetweenIterations;
        private readonly Action jobAction;
        private readonly Action onTakeTheLead;
        private readonly Action onLoseTheLead;
        private readonly ILog logger;
        private readonly ManualResetEventSlim stopSignal;
        private readonly Thread jobThread;
    }
}