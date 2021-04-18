using System;

using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;

namespace SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData
{
    [RtqTaskName("DeltaTaskData")]
    public class DeltaTaskData : ITaskDataWithTopic
    {
        public DeltaTaskData()
        {
            ChainId = Guid.NewGuid();
            FieldWithCommonName = new[] {ChainId.GetHashCode()};
        }

        public Guid ChainId { get; set; }
        public int[] FieldWithCommonName { get; set; }
        public object NonIndexableField { get; set; }
    }
}