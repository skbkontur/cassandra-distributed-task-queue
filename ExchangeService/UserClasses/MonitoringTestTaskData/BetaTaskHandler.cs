using RemoteQueue.Handling;
using RemoteQueue.Handling.HandlerResults;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;

namespace ExchangeService.UserClasses.MonitoringTestTaskData
{
    public class BetaTaskHandler : TaskHandler<BetaTaskData>
    {
        protected override HandleResult HandleTask(BetaTaskData taskData)
        {
            return new HandleResult
                {
                    FinishAction = FinishAction.Finish
                };
        }
    }
}