using System;

using JetBrains.Annotations;

using SkbKontur.EventFeeds;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.EventFeed
{
    public class EventFeedPeriodicJobRunner : IPeriodicJobRunner
    {
        public EventFeedPeriodicJobRunner(IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection,
                                          EventFeedsGraphiteLagReporter eventFeedsGraphiteLagReporter)
        {
            this.periodicJobRunnerWithLeaderElection = periodicJobRunnerWithLeaderElection;
            this.eventFeedsGraphiteLagReporter = eventFeedsGraphiteLagReporter;
        }

        public void RunPeriodicJobWithLeaderElection([NotNull] string jobName,
                                                     TimeSpan delayBetweenIterations,
                                                     [NotNull] Action jobAction,
                                                     [NotNull] Func<IRunningEventFeed> onTakeTheLead,
                                                     [NotNull] Func<IRunningEventFeed> onLoseTheLead)
        {
            periodicJobRunnerWithLeaderElection.RunPeriodicJob(jobName,
                                                               delayBetweenIterations,
                                                               jobAction,
                                                               () =>
                                                                   {
                                                                       var runningEventFeed = onTakeTheLead();
                                                                       eventFeedsGraphiteLagReporter.Start(runningEventFeed);
                                                                   },
                                                               () =>
                                                                   {
                                                                       var runningEventFeed = onLoseTheLead();
                                                                       eventFeedsGraphiteLagReporter.Stop(runningEventFeed);
                                                                   });
        }

        public void StopPeriodicJobWithLeaderElection([NotNull] string jobName)
        {
            periodicJobRunnerWithLeaderElection.StopPeriodicJob(jobName);
        }

        private readonly IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection;
        private readonly EventFeedsGraphiteLagReporter eventFeedsGraphiteLagReporter;
    }
}