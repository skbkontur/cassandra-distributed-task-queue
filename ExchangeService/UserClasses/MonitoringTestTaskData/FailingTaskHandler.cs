using System;

using RemoteQueue.Handling;
using RemoteQueue.Handling.HandlerResults;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;

namespace ExchangeService.UserClasses.MonitoringTestTaskData
{
    public class FailingTaskHandler : TaskHandler<FailingTaskData>
    {
        protected override HandleResult HandleTask(FailingTaskData taskData)
        {
            throw new Exception(string.Format("FailingTask failed: {0}", taskData));
        }
    }
}