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
        [Obsolete("todo (andrew, 20.04.2020): remove after legacy rtq tasks ttl will expire on 20.07.2020")]
        string NewQueueKeyspace { get; }

        TimeSpan TaskTtl { get; }
    }
}