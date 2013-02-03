using System;

namespace RemoteQueue.Configuration
{
    public interface IExchangeSchedulableRunner
    {
        void Start();
        void Stop();
        long GetQueueLength();
        Tuple<long, long> GetCassandraQueueLength();
        void CancelAllTasks();
    }
}