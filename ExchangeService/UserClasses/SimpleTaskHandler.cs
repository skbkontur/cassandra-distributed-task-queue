using RemoteQueue.Handling;
using RemoteQueue.Handling.HandlerResults;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas;

namespace ExchangeService.UserClasses
{
    public class SimpleTaskHandler : TaskHandler<SimpleTaskData>
    {
        protected override HandleResult HandleTask(SimpleTaskData taskData)
        {
            return new HandleResult
                {
                    FinishAction = FinishAction.Finish
                };
        }
    }
}