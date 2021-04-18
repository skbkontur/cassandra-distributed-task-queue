using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.TaskDatas
{
    [RtqTaskName("FakeFailTaskData")]
    public class FakeFailTaskData : IRtqTaskData
    {
    }
}