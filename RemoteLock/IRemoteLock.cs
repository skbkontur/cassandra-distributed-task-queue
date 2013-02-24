using System;

namespace RemoteLock
{
    public interface IRemoteLock : IDisposable
    {
        string LockId { get; }
        string ThreadId { get; }
    }
}