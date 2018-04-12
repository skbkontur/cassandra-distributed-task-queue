using RemoteQueue.Handling;

using RemoteTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData;

namespace ExchangeService.UserClasses.MonitoringTestTaskData
{
    public class GammaTaskHandler : TaskHandler<GammaTaskData>
    {
        protected override HandleResult HandleTask(GammaTaskData taskData)
        {
            return Finish();
        }
    }
}