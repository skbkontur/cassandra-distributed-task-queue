namespace RemoteQueue.Settings
{
    public interface ICassandraSettings
    {
        string QueueKeyspace { get; }
    }
}