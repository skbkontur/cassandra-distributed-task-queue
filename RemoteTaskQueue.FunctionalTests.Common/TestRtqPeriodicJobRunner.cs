using System;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

using SKBKontur.Catalogue.ServiceLib.Scheduling;

namespace RemoteTaskQueue.FunctionalTests.Common
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
                                                     [NotNull] Action jobAction,
                                                     [NotNull] Action onTakeTheLead,
                                                     [NotNull] Action onLoseTheLead)
        {
            lazyJobRunnerWithLeaderElection.Value.RunPeriodicJob(jobId, delayBetweenIterations, jobAction, onTakeTheLead, onLoseTheLead);
        }

        public void StopPeriodicJobWithLeaderElection([NotNull] string jobId)
        {
            lazyJobRunnerWithLeaderElection.Value.StopPeriodicJob(jobId);
        }

        private readonly IPeriodicTaskRunner periodicTaskRunner;
        private readonly Lazy<IPeriodicJobRunnerWithLeaderElection> lazyJobRunnerWithLeaderElection;
    }
}