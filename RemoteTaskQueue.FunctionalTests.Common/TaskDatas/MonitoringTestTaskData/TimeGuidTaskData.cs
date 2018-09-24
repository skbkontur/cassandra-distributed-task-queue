using RemoteQueue.Configuration;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.Objects.TimeBasedUuid;

namespace RemoteTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData
{
    [TaskName("TimeGuidTaskData")]
    public class TimeGuidTaskData : ITaskData
    {
        public TimeGuid Value { get; set; }
    }
}