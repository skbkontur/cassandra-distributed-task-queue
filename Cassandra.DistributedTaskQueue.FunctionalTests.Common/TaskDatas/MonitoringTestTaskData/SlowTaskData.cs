using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData
{
    [RtqTaskName("SlowTaskData")]
    public class SlowTaskData : IRtqTaskData
    {
        public int TimeMs { get; set; }
        public bool UseCounter { get; set; }
    }
}