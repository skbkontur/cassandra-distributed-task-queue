using System;
using System.Threading;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.Scheduling;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common
{
    public class TestRtqPeriodicJobRunner : IRtqPeriodicJobRunner
    {
        public TestRtqPeriodicJobRunner(IPeriodicTaskRunner periodicTaskRunner, Lazy<IPeriodicJobRunnerWithLeaderElection> lazyJobRunnerWithLeaderElection)
        {
            this.periodicTaskRunner = periodicTaskRunner;
            this.lazyJobRunnerWithLeaderElection = lazyJobRunnerWithLeaderElection;
        }

        public void RunPeriodicJob([NotNull] string jobId, TimeSpan delayBetweenIterations, [NotNull] Action jobAction)
        {
            periodicTaskRunner.Register(jobId, delayBetweenIterations, jobAction);
        }

        public void StopPeriodicJob([NotNull] string jobId)
        {
            periodicTaskRunner.Unregister(jobId, timeout : 15000);
        }

        public void RunPeriodicJobWithLeaderElection([NotNull] string jobId,
                                                     TimeSpan delayBetweenIterations,
                                                     [NotNull] Action<CancellationToken> jobAction,
                                                     [NotNull] Action onTakeTheLead,
                                                     [NotNull] Action onLoseTheLead,
                                                     CancellationToken cancellationToken)
        {
            lazyJobRunnerWithLeaderElection.Value.RunPeriodicJob(jobId,
                                                                 delayBetweenIterations,
                                                                 jobAction,
                                                                 onTakeTheLead,
                                                                 onLoseTheLead,
                                                                 cancellationToken : cancellationToken);
        }

        public void StopPeriodicJobWithLeaderElection([NotNull] string jobId)
        {
            lazyJobRunnerWithLeaderElection.Value.StopPeriodicJob(jobId);
        }

        private readonly IPeriodicTaskRunner periodicTaskRunner;
        private readonly Lazy<IPeriodicJobRunnerWithLeaderElection> lazyJobRunnerWithLeaderElection;
    }
}