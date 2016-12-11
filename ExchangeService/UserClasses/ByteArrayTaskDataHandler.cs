using RemoteQueue.Handling;

using RemoteTaskQueue.FunctionalTests.Common.TaskDatas;

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