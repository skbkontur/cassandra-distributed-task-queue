using RemoteQueue.Configuration;
using RemoteQueue.Handling;

namespace RemoteTaskQueue.FunctionalTests.Common.TaskDatas
{
    [TaskName("FakePeriodicTaskData")]
    public class FakePeriodicTaskData : ITaskData
    {
    }
}