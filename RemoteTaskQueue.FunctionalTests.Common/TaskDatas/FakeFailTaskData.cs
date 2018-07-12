using RemoteQueue.Configuration;
using RemoteQueue.Handling;

namespace RemoteTaskQueue.FunctionalTests.Common.TaskDatas
{
    [TaskName("FakeFailTaskData")]
    public class FakeFailTaskData : ITaskData
    {
    }
}