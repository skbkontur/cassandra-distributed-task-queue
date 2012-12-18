using RemoteQueue.Handling;
using RemoteQueue.Handling.HandlerResults;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas;

namespace ExchangeService.UserClasses
{
    public class ByteArrayTaskDataHandler : TaskHandler<ByteArrayTaskData>
    {
        protected override HandleResult HandleTask(ByteArrayTaskData taskData)
        {
            return new HandleResult
                {
                    FinishAction = FinishAction.Finish
                };
        }
    }
}