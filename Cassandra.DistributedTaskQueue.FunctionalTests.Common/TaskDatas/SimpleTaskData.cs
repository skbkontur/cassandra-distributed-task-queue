using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.TaskDatas
{
    [RtqTaskName("SimpleTaskData")]
    public class SimpleTaskData : IRtqTaskData
    {
    }
}