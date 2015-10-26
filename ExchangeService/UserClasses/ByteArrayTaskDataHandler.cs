using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas;

namespace ExchangeService.UserClasses
{
    public class ByteArrayTaskDataHandler : TaskHandler<ByteArrayTaskData>
    {
        protected override HandleResult HandleTask(ByteArrayTaskData taskData)
        {
            return Finish();
        }
    }
}