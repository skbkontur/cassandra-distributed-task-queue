using RemoteQueue.Handling;

using RemoteTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData;

namespace ExchangeService.UserClasses.MonitoringTestTaskData
{
    public class TimeGuidTaskHandler : TaskHandler<TimeGuidTaskData>
    {
        protected override HandleResult HandleTask(TimeGuidTaskData taskData)
        {
            return Finish();
        }
    }
}