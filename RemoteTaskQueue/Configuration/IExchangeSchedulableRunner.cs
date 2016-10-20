using RemoteQueue.Handling;

namespace RemoteQueue.Configuration
{
    public interface IExchangeSchedulableRunner
    {
        void Start();
        void Stop();
        IRemoteTaskQueue RemoteTaskQueue { get; }
    }
}