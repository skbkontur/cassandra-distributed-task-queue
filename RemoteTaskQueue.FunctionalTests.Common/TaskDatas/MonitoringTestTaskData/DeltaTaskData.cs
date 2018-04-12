using System;

using RemoteQueue.Configuration;

namespace RemoteTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData
{
    [TaskName("DeltaTaskData")]
    public class DeltaTaskData : ITaskDataWithTopic
    {
        public DeltaTaskData()
        {
            ChainId = Guid.NewGuid();
            FieldWithCommonName = new[] {ChainId.GetHashCode()};
        }

        public Guid ChainId { get; set; }
        public object FieldWithCommonName { get; set; }
    }
}