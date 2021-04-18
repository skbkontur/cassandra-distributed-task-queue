using System;

using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.TestExchangeService.UserClasses.MonitoringTestTaskData
{
    public class FailingTaskHandler : RtqTaskHandler<FailingTaskData>
    {
        protected override HandleResult HandleTask(FailingTaskData taskData)
        {
            if (Context.Attempts - 1 >= taskData.RetryCount)
            {
                return new HandleResult
                    {
                        FinishAction = FinishAction.Fatal,
                        Error = new Exception(FormatError(taskData))
                    };
            }
            return new HandleResult
                {
                    FinishAction = FinishAction.RerunAfterError,
                    Error = new Exception(FormatError(taskData))
                };
        }

        private string FormatError(FailingTaskData taskData)
        {
            return string.Format("FailingTask failed: {0}. Attempts = {1}", taskData, Context.Attempts);
        }
    }
}