using RemoteQueue.Configuration;
using RemoteQueue.Handling;

namespace RemoteTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData
{
    [TaskTopic("TestTopic")]
    public interface ITaskDataWithTopic : ITaskData
    {
    }
}