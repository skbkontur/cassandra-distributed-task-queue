using RemoteQueue.Handling;

using RemoteTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData;

namespace ExchangeService.UserClasses.MonitoringTestTaskData
{
    public class EpsilonTaskHandler : TaskHandler<EpsilonTaskData>
    {
        protected override HandleResult HandleTask(EpsilonTaskData taskData)
        {
            return Finish();
        }
    }
}