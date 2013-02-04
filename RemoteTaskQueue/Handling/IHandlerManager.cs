using System;

using RemoteQueue.LocalTasks.Scheduling;

namespace RemoteQueue.Handling
{
    public interface IHandlerManager : IPeriodicTask
    {
        void Start();
        void Stop();
        long GetQueueLength();
        Tuple<long, long> GetCassandraQueueLength();
    }
}