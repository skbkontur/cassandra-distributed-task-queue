using System;

namespace RemoteQueue.Settings
{
    public interface IRemoteTaskQueueSettings
    {
        bool EnableContinuationOptimization { get; }
        string QueueKeyspace { get; }
        string QueueKeyspaceForLock { get; }
        TimeSpan TasksTtl { get; }
        TimeSpan EventLogTtl { get; }
    }
}