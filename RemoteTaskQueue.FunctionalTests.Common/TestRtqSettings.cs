using System;

using GroboContainer.Infection;

using SkbKontur.Cassandra.DistributedTaskQueue.Settings;

namespace RemoteTaskQueue.FunctionalTests.Common
{
    [IgnoredImplementation]
    public class TestRtqSettings : IRtqSettings
    {
        public string QueueKeyspace { get; } = QueueKeyspaceName;
        public string NewQueueKeyspace { get; } = QueueKeyspaceName;
        public string QueueKeyspaceForLock { get; } = QueueKeyspaceName;
        public TimeSpan TaskTtl { get; } = StandardTestTaskTtl;
        public bool EnableContinuationOptimization { get; } = true;

        public const string QueueKeyspaceName = "TestRtqKeyspace";
        public static TimeSpan StandardTestTaskTtl = TimeSpan.FromHours(24);
    }
}