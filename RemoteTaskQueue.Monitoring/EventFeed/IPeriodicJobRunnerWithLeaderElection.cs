using System;

using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.EventFeed
{
    public interface IPeriodicJobRunnerWithLeaderElection
    {
        void RunPeriodicJob([NotNull] string jobName, TimeSpan delayBetweenIterations, [NotNull] Action jobAction, [CanBeNull] Action onTakeTheLead = null, [CanBeNull] Action onLoseTheLead = null);
        void StopPeriodicJob([NotNull] string jobName);
    }
}