using System;

namespace RemoteQueue.Handling
{
    public interface IRemoteTask
    {
        string Queue();
        string Queue(TimeSpan delay);
        string Id { get; }
    }
}