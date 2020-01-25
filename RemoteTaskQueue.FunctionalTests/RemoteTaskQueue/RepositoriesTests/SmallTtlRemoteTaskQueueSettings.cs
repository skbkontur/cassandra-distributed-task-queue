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

        public bool EnableContinuationOptimization => baseSettings.EnableContinuationOptimization;
        public string QueueKeyspace => baseSettings.QueueKeyspace;
        public string NewQueueKeyspace => baseSettings.QueueKeyspace;
        public string QueueKeyspaceForLock => baseSettings.QueueKeyspaceForLock;
        public TimeSpan TaskTtl { get; }

        private readonly IRemoteTaskQueueSettings baseSettings;
    }
}