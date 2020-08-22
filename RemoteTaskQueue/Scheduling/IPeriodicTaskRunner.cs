using System;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Scheduling
{
    public interface IPeriodicTaskRunner : IDisposable
    {
        void Register(IPeriodicTask task, TimeSpan period);
        void Unregister(string taskId, int timeout);
    }
}