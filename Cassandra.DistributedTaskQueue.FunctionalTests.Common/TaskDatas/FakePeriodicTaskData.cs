using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.TaskDatas
{
    [RtqTaskName("FakePeriodicTaskData")]
    public class FakePeriodicTaskData : IRtqTaskData
    {
    }
}