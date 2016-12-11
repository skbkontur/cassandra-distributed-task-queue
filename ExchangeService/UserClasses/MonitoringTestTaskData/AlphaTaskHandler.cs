using RemoteQueue.Handling;

using RemoteTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData;

namespace ExchangeService.UserClasses.MonitoringTestTaskData
{
    public class AlphaTaskHandler : TaskHandler<AlphaTaskData>
    {
        protected override HandleResult HandleTask(AlphaTaskData taskData)
        {
            return Finish();
        }
    }
}