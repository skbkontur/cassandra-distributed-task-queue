using System;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
{
    [Obsolete("Use with caution")]
    internal interface IRtqBackdoor
    {
        void ResetTicksHolderInMemoryState();

        void ChangeTaskTtl(TimeSpan ttl);
    }
}