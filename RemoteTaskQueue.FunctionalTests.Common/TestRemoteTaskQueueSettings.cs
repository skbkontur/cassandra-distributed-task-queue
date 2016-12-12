using System;

using GroboContainer.Infection;

using RemoteQueue.Settings;

namespace RemoteTaskQueue.FunctionalTests.Common
{
    [IgnoredImplementation]
    public class TestRemoteTaskQueueSettings : IRemoteTaskQueueSettings
    {
        public string QueueKeyspace { get { return QueueKeyspaceName; } }
        public string QueueKeyspaceForLock { get { return QueueKeyspaceName; } }
        public TimeSpan TaskTtl { get { return StandardTestTaskTtl; } }
        public bool EnableContinuationOptimization { get { return true; } }

        public const string QueueKeyspaceName = "TestRtqKeyspace";
        public static TimeSpan StandardTestTaskTtl = TimeSpan.FromHours(24);
    }
}