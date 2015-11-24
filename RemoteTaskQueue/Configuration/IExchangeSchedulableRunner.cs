using System;

namespace RemoteQueue.Configuration
{
    public interface IExchangeSchedulableRunner
    {
        void Start();
        void Stop();
        Tuple<long, long> GetQueueLength();
    }
}