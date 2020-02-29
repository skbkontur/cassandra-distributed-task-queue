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

        [NotNull]
        [Obsolete("// todo (andrew, 01.03.2020): remove after avk/rtqLock release")]
        string QueueKeyspaceForLock { get; }

        TimeSpan TaskTtl { get; }
    }
}