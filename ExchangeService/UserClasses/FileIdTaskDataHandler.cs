using System;

using RemoteQueue.Handling;
using RemoteQueue.Handling.HandlerResults;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas;

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