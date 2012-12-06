using System;

namespace RemoteQueue.LocalTasks.Scheduling
{
    public interface IPeriodicTaskRunner
    {
        void Register(IPeriodicTask task, TimeSpan period);
        void Unregister(string taskId, int timeout);
    }
}