using System;

namespace RemoteQueue.Handling
{
    public interface IRemoteTask
    {
        void Queue();
        void Queue(TimeSpan delay);
        string Id { get; }
    }
}