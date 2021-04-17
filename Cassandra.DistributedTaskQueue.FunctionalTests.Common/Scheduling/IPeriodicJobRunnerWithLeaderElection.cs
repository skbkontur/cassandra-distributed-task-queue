using System;
using System.Threading;

using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.Scheduling
{
    public interface IPeriodicJobRunnerWithLeaderElection
    {
        void RunPeriodicJob([NotNull] string jobName,
                            TimeSpan delayBetweenIterations,
                            [NotNull] Action<CancellationToken> jobAction,
                            [CanBeNull] Action onTakeTheLead = null,
                            [CanBeNull] Action onLoseTheLead = null,
                            TimeSpan? leaderAcquisitionAttemptDelay = null,
                            CancellationToken cancellationToken = default);

        void StopPeriodicJob([NotNull] string jobName);
    }
}