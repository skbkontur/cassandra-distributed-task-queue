using System;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

using SKBKontur.Catalogue.ServiceLib.Scheduling;

namespace RemoteTaskQueue.FunctionalTests.Common
{
    public class TestRtqPeriodicJobRunner : IRtqPeriodicJobRunner
    {
        public TestRtqPeriodicJobRunner(IPeriodicTaskRunner periodicTaskRunner, IPeriodicJobRunnerWithLeaderElection jobRunnerWithLeaderElection)
        {
            this.periodicTaskRunner = periodicTaskRunner;
            this.jobRunnerWithLeaderElection = jobRunnerWithLeaderElection;
        }

        public void RunPeriodicJob(string jobId, TimeSpan delayBetweenIterations, Action jobAction)
        {
            periodicTaskRunner.Register(jobId, delayBetweenIterations, jobAction);
        }

        public void StopPeriodicJob(string jobId)
        {
            periodicTaskRunner.Unregister(jobId, timeout : 15000);
        }

        public void RunPeriodicJobWithLeaderElection(string jobId, TimeSpan delayBetweenIterations, Action jobAction, Action onTakeTheLead, Action onLoseTheLead)
        {
            jobRunnerWithLeaderElection.RunPeriodicJob(jobId, delayBetweenIterations, jobAction, onTakeTheLead, onLoseTheLead);
        }

        public void StopPeriodicJobWithLeaderElection(string jobId)
        {
            jobRunnerWithLeaderElection.StopPeriodicJob(jobId);
        }

        private readonly IPeriodicTaskRunner periodicTaskRunner;
        private readonly IPeriodicJobRunnerWithLeaderElection jobRunnerWithLeaderElection;
    }
}