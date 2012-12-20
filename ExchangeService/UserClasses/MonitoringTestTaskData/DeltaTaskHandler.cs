using RemoteQueue.Handling;
using RemoteQueue.Handling.HandlerResults;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;

namespace ExchangeService.UserClasses.MonitoringTestTaskData
{
    public class DeltaTaskHandler : TaskHandler<DeltaTaskData>
    {
        protected override HandleResult HandleTask(DeltaTaskData taskData)
        {
            return new HandleResult
                {
                    FinishAction = FinishAction.Finish
                };
        }
    }
}