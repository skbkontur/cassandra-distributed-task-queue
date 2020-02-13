using System;

using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;

namespace RemoteTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData
{
    [RtqTaskName("GammaTaskData")]
    public class GammaTaskData : ITaskDataWithTopic
    {
        public GammaTaskData()
        {
            ChainId = Guid.NewGuid();
            FieldWithCommonName = ChainId.ToString();
        }

        public Guid ChainId { get; set; }
        public string FieldWithCommonName { get; set; }
        public ITaskDataDetails NonIndexableField { get; set; }
    }
}