using RemoteTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace ExchangeService.UserClasses.MonitoringTestTaskData
{
    public class TimeGuidTaskHandler : RtqTaskHandler<TimeGuidTaskData>
    {
        protected override HandleResult HandleTask(TimeGuidTaskData taskData)
        {
            return Finish();
        }
    }
}