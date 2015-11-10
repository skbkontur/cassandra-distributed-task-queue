namespace RemoteQueue.Settings
{
    public interface IRemoteTaskQueueSettings
    {
        bool EnableContinuationOptimization { get; }
    }
}