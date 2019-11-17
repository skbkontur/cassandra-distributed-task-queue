using RemoteQueue.Configuration;
using RemoteQueue.Handling;

using SkbKontur.Cassandra.TimeBasedUuid;

namespace RemoteTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData
{
    [TaskName("TimeGuidTaskData")]
    public class TimeGuidTaskData : ITaskData
    {
        public TimeGuid Value { get; set; }
    }
}