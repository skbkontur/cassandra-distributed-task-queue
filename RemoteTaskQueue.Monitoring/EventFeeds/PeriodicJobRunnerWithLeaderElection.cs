using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedLock;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.EventFeeds
{
    public class PeriodicJobRunnerWithLeaderElection : IPeriodicJobRunnerWithLeaderElection, IDisposable
    {
        public PeriodicJobRunnerWithLeaderElection(IRemoteLockCreator remoteLockCreator, ILog logger)
        {
            this.remoteLockCreator = remoteLockCreator;
            this.logger = logger.ForContext(nameof(PeriodicJobRunnerWithLeaderElection));
        }

        public void RunPeriodicJob([NotNull] string jobName, TimeSpan delayBetweenIterations, [NotNull] Action jobAction, [CanBeNull] Action onTakeTheLead = null, [CanBeNull] Action onLoseTheLead = null)
        {
            lock (locker)
            {
                if (isDisposed)
                    throw new ObjectDisposedException("PeriodicJobRunnerWithLeaderElection is already disposed");
                if (runningJobs.ContainsKey(jobName))
                    throw new InvalidOperationException($"Job is already running: {jobName}");
                var runningJob = new PeriodicJobWithLeaderElection(remoteLockCreator, jobName, delayBetweenIterations, jobAction, onTakeTheLead, onLoseTheLead, logger);
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
                job.Stop();
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
                    runningJob.Stop();
                isDisposed = true;
            }
        }

        private bool isDisposed;
        private readonly IRemoteLockCreator remoteLockCreator;
        private readonly ILog logger;
        private readonly object locker = new object();
        private readonly Dictionary<string, PeriodicJobWithLeaderElection> runningJobs = new Dictionary<string, PeriodicJobWithLeaderElection>();
    }
}