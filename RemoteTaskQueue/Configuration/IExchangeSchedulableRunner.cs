using System;

using RemoteQueue.Handling;

namespace RemoteQueue.Configuration
{
    public interface IExchangeSchedulableRunner
    {
        void Start();
        void Stop();
        Tuple<long, long> GetQueueLength();
        IRemoteTaskQueue RemoteTaskQueue { get; }
    }
}