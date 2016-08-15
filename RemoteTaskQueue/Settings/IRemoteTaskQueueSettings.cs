namespace RemoteQueue.Settings
{
    public interface IRemoteTaskQueueSettings
    {
        bool EnableContinuationOptimization { get; }
        string QueueKeyspace { get; }
        string QueueKeyspaceForLock { get; }
        bool RemoteLockMigrationEnabled { get; }
    }
}