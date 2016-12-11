using RemoteQueue.Configuration;
using RemoteQueue.Handling;

namespace RemoteTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData
{
    [TaskName("SlowTaskData")]
    public class SlowTaskData : ITaskData
    {
        public int TimeMs { get; set; }
        public bool UseCounter { get; set; }
    }
}