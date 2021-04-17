using System;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.TaskDatas
{
    [RtqTaskName("FakeMixedPeriodicAndFailTaskData")]
    public class FakeMixedPeriodicAndFailTaskData : IRtqTaskData
    {
        public FakeMixedPeriodicAndFailTaskData(TimeSpan rerunAfter, [CanBeNull] int[] failWhenCounterEqualTo)
        {
            RerunAfter = rerunAfter;
            FailCounterValues = failWhenCounterEqualTo;
        }

        public TimeSpan RerunAfter { get; private set; }

        [CanBeNull]
        public int[] FailCounterValues { get; private set; }
    }
}