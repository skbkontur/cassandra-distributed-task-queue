using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace RemoteTaskQueue.FunctionalTests.Common.TaskDatas
{
    [RtqTaskName("FakePeriodicTaskData")]
    public class FakePeriodicTaskData : IRtqTaskData
    {
    }
}