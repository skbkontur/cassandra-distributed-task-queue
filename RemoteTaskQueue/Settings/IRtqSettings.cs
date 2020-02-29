using System;

using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Settings
{
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