using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData
{
    [RtqTaskName("TimeGuidTaskData")]
    public class TimeGuidTaskData : IRtqTaskData
    {
        public TimeGuid Value { get; set; }
    }
}