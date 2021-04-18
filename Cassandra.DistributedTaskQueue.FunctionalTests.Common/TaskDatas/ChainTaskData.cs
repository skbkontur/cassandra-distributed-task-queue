using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.TaskDatas
{
    [RtqTaskName("ChainTaskData")]
    public class ChainTaskData : IRtqTaskData
    {
        public string ChainName { get; set; }
        public int ChainPosition { get; set; }
        public string LoggingTaskIdKey { get; set; }
    }
}