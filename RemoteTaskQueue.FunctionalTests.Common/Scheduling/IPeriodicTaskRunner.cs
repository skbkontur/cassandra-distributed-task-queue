using System;

namespace RemoteTaskQueue.FunctionalTests.Common.Scheduling
{
    public interface IPeriodicTaskRunner : IDisposable
    {
        void Register(IPeriodicTask task, TimeSpan period);
        void Unregister(string taskId, int timeout);
    }
}