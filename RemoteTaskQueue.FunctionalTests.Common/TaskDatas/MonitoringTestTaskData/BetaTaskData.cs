using RemoteQueue.Configuration;

namespace RemoteTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData
{
    [TaskName("BetaTaskData")]
    public class BetaTaskData : ITaskDataWithTopic
    {
        public bool IsProcess { get; set; }
    }
}