using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.TestExchangeService.UserClasses.MonitoringTestTaskData
{
    public class TimeGuidTaskHandler : RtqTaskHandler<TimeGuidTaskData>
    {
        protected override HandleResult HandleTask(TimeGuidTaskData taskData)
        {
            return Finish();
        }
    }
}