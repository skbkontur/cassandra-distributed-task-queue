using System;

namespace RemoteQueue.Settings
{
    public interface IExchangeSchedulableRunnerSettings
    {
        TimeSpan PeriodicInterval { get; }
        int MaxRunningTasksCount { get; }
        int MaxRunningContinuationsCount { get; }
    }
}