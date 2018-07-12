using System;

using GroboContainer.Infection;

using RemoteQueue.Settings;

namespace RemoteTaskQueue.FunctionalTests.RemoteTaskQueue.RepositoriesTests
{
    [IgnoredImplementation]
    public class SmallTtlRemoteTaskQueueSettings : IRemoteTaskQueueSettings
    {
        public SmallTtlRemoteTaskQueueSettings(IRemoteTaskQueueSettings baseSettings, TimeSpan taskTtl)
        {
            this.baseSettings = baseSettings;
            TaskTtl = taskTtl;
        }

        public bool EnableContinuationOptimization { get { return baseSettings.EnableContinuationOptimization; } }
        public string QueueKeyspace { get { return baseSettings.QueueKeyspace; } }
        public string QueueKeyspaceForLock { get { return baseSettings.QueueKeyspaceForLock; } }
        public TimeSpan TaskTtl { get; private set; }

        private readonly IRemoteTaskQueueSettings baseSettings;
    }
}