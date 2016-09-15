using System;

using GroboContainer.Infection;

using RemoteQueue.Settings;

namespace FunctionalTests.RepositoriesTests
{
    [IgnoredImplementation]
    public class SmallTtlRemoteTaskQueueSettings : IRemoteTaskQueueSettings
    {
        private readonly IRemoteTaskQueueSettings baseSettings;
        public SmallTtlRemoteTaskQueueSettings(IRemoteTaskQueueSettings baseSettings, TimeSpan tasksTtl)
        {
            this.baseSettings = baseSettings;
            TasksTtl = tasksTtl;
        }

        public bool EnableContinuationOptimization { get { return baseSettings.EnableContinuationOptimization; } }
        public string QueueKeyspace { get { return baseSettings.QueueKeyspace; } }
        public string QueueKeyspaceForLock { get { return baseSettings.QueueKeyspaceForLock; } }
        public TimeSpan TasksTtl { get; private set; }
        public TimeSpan EventLogTtl { get { return baseSettings.EventLogTtl; } }
    }
}