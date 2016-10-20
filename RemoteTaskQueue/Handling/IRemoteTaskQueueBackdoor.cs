using System;

namespace RemoteQueue.Handling
{
    [Obsolete("Use with caution")]
    public interface IRemoteTaskQueueBackdoor
    {
        void ResetTicksHolderInMemoryState();

        void ChangeTaskTtl(TimeSpan ttl);
    }
}