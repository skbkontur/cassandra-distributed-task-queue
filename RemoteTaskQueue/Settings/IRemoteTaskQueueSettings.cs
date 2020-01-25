using System;

using JetBrains.Annotations;

namespace RemoteQueue.Settings
{
    public interface IRemoteTaskQueueSettings
    {
        bool EnableContinuationOptimization { get; }

        [NotNull]
        string QueueKeyspace { get; }

        [NotNull]
        string NewQueueKeyspace { get; }

        [NotNull]
        string QueueKeyspaceForLock { get; }

        TimeSpan TaskTtl { get; }
    }
}