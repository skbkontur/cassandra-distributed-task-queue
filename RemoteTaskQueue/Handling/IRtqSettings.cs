using System;

using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
{
    [PublicAPI]
    public interface IRtqSettings
    {
        bool EnableContinuationOptimization { get; }

        [NotNull]
        string QueueKeyspace { get; }

        [NotNull]
        string NewQueueKeyspace { get; }

        TimeSpan TaskTtl { get; }
    }
}