namespace RemoteQueue.Settings
{
    public interface ICassandraSettings
    {
        string QueueKeyspace { get; }
        string QueueKeyspaceForLock { get; }
        bool RemoteLockMigrationEnabled { get; }
    }
}