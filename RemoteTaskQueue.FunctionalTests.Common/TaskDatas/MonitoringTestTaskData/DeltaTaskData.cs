using RemoteQueue.Configuration;

namespace RemoteTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData
{
    [TaskName("DeltaTaskData")]
    public class DeltaTaskData : ITaskDataWithTopic
    {
    }
}