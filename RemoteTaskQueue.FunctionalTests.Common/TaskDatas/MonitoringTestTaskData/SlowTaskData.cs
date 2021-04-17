using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace RemoteTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData
{
    [RtqTaskName("SlowTaskData")]
    public class SlowTaskData : IRtqTaskData
    {
        public int TimeMs { get; set; }
        public bool UseCounter { get; set; }
    }
}