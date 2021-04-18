using System;

using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;

namespace SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData
{
    [RtqTaskName("AlphaTaskData")]
    public class AlphaTaskData : ITaskDataWithTopic
    {
        public AlphaTaskData()
        {
            ChainId = Guid.NewGuid();
            FieldWithCommonName = ChainId.GetHashCode();
        }

        public Guid ChainId { get; set; }
        public int FieldWithCommonName { get; set; }
        public TaskDataDetailsBase NonIndexableField { get; set; }
    }
}