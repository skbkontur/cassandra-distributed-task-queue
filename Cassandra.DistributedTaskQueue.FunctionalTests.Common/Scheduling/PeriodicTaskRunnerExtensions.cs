using System;

using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.Scheduling
{
    public static class PeriodicTaskRunnerExtensions
    {
        public static void Register([NotNull] this IPeriodicTaskRunner periodicTaskRunner, [NotNull] string taskId, TimeSpan period, [NotNull] Action taskAction)
        {
            periodicTaskRunner.Register(new ActionPeriodicTask(taskId, taskAction), period);
        }

        public static void Unregister([NotNull] this IPeriodicTaskRunner periodicTaskRunner, [NotNull] string taskId, TimeSpan timeout)
        {
            periodicTaskRunner.Unregister(taskId, (int)timeout.TotalMilliseconds);
        }
    }
}