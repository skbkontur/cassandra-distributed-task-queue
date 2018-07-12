using System;

using JetBrains.Annotations;

using RemoteQueue.Configuration;
using RemoteQueue.Handling;

namespace RemoteTaskQueue.FunctionalTests.Common.TaskDatas
{
    [TaskName("FakeMixedPeriodicAndFailTaskData")]
    public class FakeMixedPeriodicAndFailTaskData : ITaskData
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