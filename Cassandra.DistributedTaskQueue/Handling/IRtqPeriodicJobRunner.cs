using System;

using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
{
    [PublicAPI]
    public interface IRtqPeriodicJobRunner
    {
        void RunPeriodicJob([NotNull] string jobId,
                            TimeSpan delayBetweenIterations,
                            [NotNull] Action jobAction);

        void StopPeriodicJob([NotNull] string jobId);

        void RunPeriodicJobWithLeaderElection([NotNull] string jobId,
                                              TimeSpan delayBetweenIterations,
                                              [NotNull] Action jobAction,
                                              [NotNull] Action onTakeTheLead,
                                              [NotNull] Action onLoseTheLead);

        void StopPeriodicJobWithLeaderElection([NotNull] string jobId);
    }
}