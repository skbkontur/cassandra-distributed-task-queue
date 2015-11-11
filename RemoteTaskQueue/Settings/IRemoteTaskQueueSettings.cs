namespace RemoteQueue.Settings
{
    public interface IRemoteTaskQueueSettings
    {
        bool UseRemoteLocker { get; }
        bool EnableContinuationOptimization { get; }
    }
}