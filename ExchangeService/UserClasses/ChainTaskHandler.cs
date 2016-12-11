using RemoteQueue.Handling;

using RemoteTaskQueue.FunctionalTests.Common.TaskDatas;

namespace ExchangeService.UserClasses
{
    public class ChainTaskHandler : TaskHandler<ChainTaskData>
    {
        public ChainTaskHandler(IRemoteTaskQueue remoteTaskQueue, ITestTaskLogger logger)
        {
            this.remoteTaskQueue = remoteTaskQueue;
            this.logger = logger;
        }

        protected override HandleResult HandleTask(ChainTaskData taskData)
        {
            logger.Log(taskData.LoggingTaskIdKey, Context.Id);
            if(taskData.ChainPosition == 9)
                return Finish();
            remoteTaskQueue.CreateTask(new ChainTaskData
                {
                    ChainPosition = taskData.ChainPosition + 1,
                    ChainName = taskData.ChainName,
                    LoggingTaskIdKey = taskData.LoggingTaskIdKey
                }).Queue();
            return Finish();
        }

        private readonly IRemoteTaskQueue remoteTaskQueue;
        private readonly ITestTaskLogger logger;
    }
}