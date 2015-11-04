using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace RemoteQueue.Settings
{
    public interface IExchangeSchedulableRunnerSettings
    {
        [CanBeNull]
        HashSet<string> Topics { get; }

        TimeSpan PeriodicInterval { get; }
        int MaxRunningTasksCount { get; }
        int MaxRunningContinuationsCount { get; }
    }
}