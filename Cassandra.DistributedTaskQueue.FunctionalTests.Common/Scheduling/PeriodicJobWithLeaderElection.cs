using System;
using System.Diagnostics;
using System.Net;
using System.Threading;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedLock;
using SkbKontur.Cassandra.TimeBasedUuid;
using SkbKontur.Graphite.Client;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.Scheduling
{
    public class PeriodicJobWithLeaderElection : IDisposable
    {
        public PeriodicJobWithLeaderElection(
            [NotNull] IRemoteLockCreator remoteLockCreator,
            [NotNull] IPeriodicTaskRunner periodicTaskRunner,
            [NotNull] IGraphiteClient graphiteClient,
            [NotNull] ILog logger,
            [NotNull] string jobName,
            TimeSpan delayBetweenIterations,
            TimeSpan leaderAcquisitionAttemptDelay,
            [NotNull] Action<CancellationToken> jobAction,
            [CanBeNull] Action onTakeTheLead,
            [CanBeNull] Action onLoseTheLead,
            CancellationToken cancellationToken)
        {
            this.remoteLockCreator = remoteLockCreator;
            this.periodicTaskRunner = periodicTaskRunner;
            this.graphiteClient = graphiteClient;
            this.logger = logger;
            this.jobName = jobName;
            this.delayBetweenIterations = delayBetweenIterations;
            this.leaderAcquisitionAttemptDelay = leaderAcquisitionAttemptDelay;
            this.jobAction = jobAction;
            this.onTakeTheLead = onTakeTheLead;
            this.onLoseTheLead = onLoseTheLead;
            isDisposed = false;
            jobCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            leaderActivityReportingJobId = $"leaderActivityReportingJobId-{jobName}";
            leaderActivityReportingGraphitePath = $"SubSystem.LeaderElection.{jobName}.{Dns.GetHostName()}";

            jobThread = new Thread(() => ThreadProc(jobCancellationTokenSource.Token))
                {
                    Name = jobName,
                    IsBackground = true,
                };
            jobThread.Start();
            this.logger.Info("Job thread has started for {JobName}", new {JobName = jobName});
        }

        public void Dispose()
        {
            lock (syncObject)
            {
                if (isDisposed)
                    return;

                if (!jobCancellationTokenSource.IsCancellationRequested)
                    jobCancellationTokenSource.Cancel();

                jobThread.Join();
                logger.Info("Job thread has stopped for: {JobName}", new {JobName = jobName});

                jobCancellationTokenSource.Dispose();

                isDisposed = true;
            }
        }

        private void ThreadProc(CancellationToken jobCancellationToken)
        {
            do
            {
                try
                {
                    var lockId = $"LeaderElection/{jobName}";
                    if (!remoteLockCreator.TryGetLock(lockId, out var leaderLock))
                        continue;

                    using var leaderCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(jobCancellationToken, leaderLock.LockAliveToken);

                    logger.Info("Leadership acquired for: {JobName}", new {JobName = jobName});
                    using (leaderLock)
                    {
                        periodicTaskRunner.Register(leaderActivityReportingJobId,
                                                    period : TimeSpan.FromMilliseconds(900),
                                                    () => graphiteClient.Send(leaderActivityReportingGraphitePath, 1, Timestamp.Now.ToDateTime()));
                        onTakeTheLead?.Invoke();
                        try
                        {
                            LeaderThreadProc(leaderCancellationTokenSource.Token);
                        }
                        finally
                        {
                            onLoseTheLead?.Invoke();
                            periodicTaskRunner.Unregister(leaderActivityReportingJobId, timeout : TimeSpan.FromSeconds(15));
                            graphiteClient.Send(leaderActivityReportingGraphitePath, 0, Timestamp.Now.ToDateTime());
                        }
                    }
                    logger.Info("Leadership released for: {JobName}", new {JobName = jobName});
                }
                catch (Exception e)
                {
                    logger.Error(e, "Leadership lost with unhandled exception on job thread for: {JobName}", new {JobName = jobName});
                }
            } while (!jobCancellationToken.WaitHandle.WaitOne(leaderAcquisitionAttemptDelay));
        }

        private void LeaderThreadProc(CancellationToken leaderCancellationToken)
        {
            Stopwatch iterationStopwatch;
            do
            {
                iterationStopwatch = Stopwatch.StartNew();
                try
                {
                    jobAction(leaderCancellationToken);
                }
                catch (OperationCanceledException)
                {
                }
            } while (!leaderCancellationToken.WaitHandle.WaitOne(DateTimeMath.Max(TimeSpan.Zero, delayBetweenIterations - iterationStopwatch.Elapsed)));
        }

        private readonly IRemoteLockCreator remoteLockCreator;
        private readonly IPeriodicTaskRunner periodicTaskRunner;
        private readonly IGraphiteClient graphiteClient;
        private readonly ILog logger;
        private readonly string jobName;
        private readonly TimeSpan delayBetweenIterations;
        private readonly TimeSpan leaderAcquisitionAttemptDelay;
        private readonly Action<CancellationToken> jobAction;
        private readonly Action onTakeTheLead;
        private readonly Action onLoseTheLead;
        private readonly string leaderActivityReportingJobId;
        private readonly string leaderActivityReportingGraphitePath;
        private readonly Thread jobThread;
        private readonly object syncObject = new object();
        private readonly CancellationTokenSource jobCancellationTokenSource;
        private bool isDisposed;
    }
}