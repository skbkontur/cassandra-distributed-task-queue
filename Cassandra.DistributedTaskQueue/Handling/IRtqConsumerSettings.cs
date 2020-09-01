using System;

using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
{
    [PublicAPI]
    public interface IRtqConsumerSettings
    {
        TimeSpan PeriodicInterval { get; }
        int MaxRunningTasksCount { get; }
        int MaxRunningContinuationsCount { get; }
    }
}