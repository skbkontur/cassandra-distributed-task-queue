using System;
using System.Collections.Generic;
using System.Threading;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedLock;
using SkbKontur.Graphite.Client;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.Scheduling
{
    public class PeriodicJobRunnerWithLeaderElection : IPeriodicJobRunnerWithLeaderElection, IDisposable
    {
        public PeriodicJobRunnerWithLeaderElection(IRemoteLockCreator remoteLockCreator, IPeriodicTaskRunner periodicTaskRunner, IGraphiteClient graphiteClient, ILog logger)
        {
            this.remoteLockCreator = remoteLockCreator;
            this.periodicTaskRunner = periodicTaskRunner;
            this.graphiteClient = graphiteClient;
            this.logger = logger.ForContext(nameof(PeriodicJobRunnerWithLeaderElection));
        }

        public void RunPeriodicJob([NotNull] string jobName,
                                   TimeSpan delayBetweenIterations,
                                   [NotNull] Action<CancellationToken> jobAction,
                                   [CanBeNull] Action onTakeTheLead = null,
                                   [CanBeNull] Action onLoseTheLead = null,
                                   TimeSpan? leaderAcquisitionAttemptDelay = null,
                                   CancellationToken cancellationToken = default)
        {
            lock (locker)
            {
                if (isDisposed)
                    throw new ObjectDisposedException("PeriodicJobRunnerWithLeaderElection is already disposed");
                if (runningJobs.ContainsKey(jobName))
                    throw new InvalidOperationException($"Job is already running: {jobName}");
                var runningJob = new PeriodicJobWithLeaderElection(remoteLockCreator,
                                                                   periodicTaskRunner,
                                                                   graphiteClient,
                                                                   logger,
                                                                   jobName,
                                                                   delayBetweenIterations,
                                                                   leaderAcquisitionAttemptDelay ?? TimeSpan.FromSeconds(10),
                                                                   jobAction,
                                                                   onTakeTheLead,
                                                                   onLoseTheLead,
                                                                   cancellationToken);
                runningJobs.Add(jobName, runningJob);
            }
        }

        public void StopPeriodicJob([NotNull] string jobName)
        {
            lock (locker)
            {
                if (isDisposed)
                    throw new ObjectDisposedException("PeriodicJobRunnerWithLeaderElection is already disposed");
                if (!runningJobs.ContainsKey(jobName))
                    throw new InvalidOperationException($"Job {jobName} does not exist");
                var job = runningJobs[jobName];
                job.Dispose();
                runningJobs.Remove(jobName);
            }
        }

        public void Dispose()
        {
            lock (locker)
            {
                if (isDisposed)
                    return;

                foreach (var runningJob in runningJobs.Values)
                    runningJob.Dispose();

                isDisposed = true;
            }
        }

        private bool isDisposed;
        private readonly IRemoteLockCreator remoteLockCreator;
        private readonly IPeriodicTaskRunner periodicTaskRunner;
        private readonly IGraphiteClient graphiteClient;
        private readonly ILog logger;
        private readonly object locker = new object();
        private readonly Dictionary<string, PeriodicJobWithLeaderElection> runningJobs = new Dictionary<string, PeriodicJobWithLeaderElection>();
    }
}