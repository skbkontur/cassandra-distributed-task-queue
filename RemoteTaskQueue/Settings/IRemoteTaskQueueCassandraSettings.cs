namespace RemoteQueue.Settings
{
    public interface IRemoteTaskQueueCassandraSettings
    {
        string QueueKeyspace { get; }
    }
}