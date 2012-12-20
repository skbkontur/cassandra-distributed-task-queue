using RemoteQueue.Handling;
using RemoteQueue.Handling.HandlerResults;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;

namespace ExchangeService.UserClasses.MonitoringTestTaskData
{
    public class AlphaTaskHandler : TaskHandler<AlphaTaskData>
    {
        protected override HandleResult HandleTask(AlphaTaskData taskData)
        {
            return new HandleResult
                {
                    FinishAction = FinishAction.Finish
                };
        }
    }
}