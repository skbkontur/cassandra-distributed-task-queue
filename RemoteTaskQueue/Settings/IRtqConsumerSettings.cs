using System;

namespace RemoteQueue.Settings
{
    public interface IRtqConsumerSettings
    {
        TimeSpan PeriodicInterval { get; }
        int MaxRunningTasksCount { get; }
        int MaxRunningContinuationsCount { get; }
    }
}