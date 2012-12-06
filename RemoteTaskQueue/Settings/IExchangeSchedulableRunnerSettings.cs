using System;

namespace RemoteQueue.Settings
{
    public interface IExchangeSchedulableRunnerSettings
    {
        TimeSpan PeriodicInterval { get; }
        int MaxRunningTasksCount { get; }
        int ShardsCount { get; }
        int ShardIndex { get; }
    }
}