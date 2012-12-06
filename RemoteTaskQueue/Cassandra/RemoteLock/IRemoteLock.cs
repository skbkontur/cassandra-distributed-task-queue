using System;

namespace RemoteQueue.Cassandra.RemoteLock
{
    public interface IRemoteLock : IDisposable
    {
        string LockId { get; }
        string ThreadId { get; }
    }
}