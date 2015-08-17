using System;

namespace RemoteQueue.Handling
{
    public interface IRemoteTask
    {
        string Id { get; }
        string Queue();
        string Queue(TimeSpan delay);
    }
}