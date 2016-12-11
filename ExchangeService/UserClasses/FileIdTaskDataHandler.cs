using System;

using RemoteQueue.Handling;

using RemoteTaskQueue.FunctionalTests.Common.TaskDatas;

namespace ExchangeService.UserClasses
{
    public class FileIdTaskDataHandler : TaskHandler<FileIdTaskData>
    {
        protected override HandleResult HandleTask(FileIdTaskData taskData)
        {
            return new HandleResult
                {
                    FinishAction = FinishAction.Fatal,
                    Error = new Exception("Bad")
                };
        }
    }
}