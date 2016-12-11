using RemoteQueue.Handling;

using RemoteTaskQueue.FunctionalTests.Common.TaskDatas;

namespace ExchangeService.UserClasses
{
    public class ByteArrayAndNestedTaskHandler : TaskHandler<ByteArrayAndNestedTaskData>
    {
        protected override HandleResult HandleTask(ByteArrayAndNestedTaskData taskData)
        {
            return Finish();
        }
    }
}