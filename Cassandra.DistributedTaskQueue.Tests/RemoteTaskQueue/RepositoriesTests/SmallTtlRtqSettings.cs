﻿using System;

using GroboContainer.Infection;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests.RemoteTaskQueue.RepositoriesTests
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
        public TimeSpan TaskTtl { get; }

        private readonly IRtqSettings baseSettings;
    }
}