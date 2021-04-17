using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;

namespace RemoteTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData
{
    [RtqTaskName("BetaTaskData")]
    public class BetaTaskData : ITaskDataWithTopic
    {
        public bool IsProcess { get; set; }
    }
}