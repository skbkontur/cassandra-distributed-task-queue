using System;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Settings
{
    public interface IRtqConsumerSettings
    {
        TimeSpan PeriodicInterval { get; }
        int MaxRunningTasksCount { get; }
        int MaxRunningContinuationsCount { get; }
    }
}