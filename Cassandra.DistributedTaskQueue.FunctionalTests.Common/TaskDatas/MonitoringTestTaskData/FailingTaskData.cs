using System;

using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData
{
    [RtqTaskName("FailingTaskData")]
    public class FailingTaskData : IRtqTaskData
    {
        public Guid UniqueData { get; set; }

        public int RetryCount { get; set; }

        public override string ToString()
        {
            return string.Format("UniqueData: {0}", UniqueData);
        }
    }
}