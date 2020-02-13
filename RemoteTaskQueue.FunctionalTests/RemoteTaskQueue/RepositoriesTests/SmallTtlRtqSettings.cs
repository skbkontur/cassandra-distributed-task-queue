using System;

using GroboContainer.Infection;

using SkbKontur.Cassandra.DistributedTaskQueue.Settings;

namespace RemoteTaskQueue.FunctionalTests.RemoteTaskQueue.RepositoriesTests
{
    [IgnoredImplementation]
    public class SmallTtlRtqSettings : IRtqSettings
    {
        public SmallTtlRtqSettings(IRtqSettings baseSettings, TimeSpan taskTtl)
        {
            this.baseSettings = baseSettings;
            TaskTtl = taskTtl;
        }

        public bool EnableContinuationOptimization => baseSettings.EnableContinuationOptimization;
        public string QueueKeyspace => baseSettings.QueueKeyspace;
        public string NewQueueKeyspace => baseSettings.QueueKeyspace;
        public string QueueKeyspaceForLock => baseSettings.QueueKeyspaceForLock;
        public TimeSpan TaskTtl { get; }

        private readonly IRtqSettings baseSettings;
    }
}