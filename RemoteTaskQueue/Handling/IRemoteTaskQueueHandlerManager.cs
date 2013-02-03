using System;

using RemoteQueue.LocalTasks.Scheduling;

namespace RemoteQueue.Handling
{
    public interface IRemoteTaskQueueHandlerManager : IPeriodicTask
    {
        void Start();
        void Stop();
        long GetQueueLength();
        Tuple<long, long> GetCassandraQueueLength();
        void CancelAllTasks();
    }
}