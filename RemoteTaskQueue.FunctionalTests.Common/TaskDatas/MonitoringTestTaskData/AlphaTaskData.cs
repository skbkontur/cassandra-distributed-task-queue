using System;

using RemoteQueue.Configuration;

namespace RemoteTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData
{
    [TaskName("AlphaTaskData")]
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