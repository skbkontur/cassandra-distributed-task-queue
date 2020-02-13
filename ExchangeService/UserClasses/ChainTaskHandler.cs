using RemoteTaskQueue.FunctionalTests.Common.ConsumerStateImpl;
using RemoteTaskQueue.FunctionalTests.Common.TaskDatas;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace ExchangeService.UserClasses
{
    public class ChainTaskHandler : RtqTaskHandler<ChainTaskData>
    {
        public ChainTaskHandler(IRtqTaskProducer taskProducer, ITestTaskLogger logger)
        {
            this.taskProducer = taskProducer;
            this.logger = logger;
        }

        protected override HandleResult HandleTask(ChainTaskData taskData)
        {
            logger.Log(taskData.LoggingTaskIdKey, Context.Id);
            if (taskData.ChainPosition == 9)
                return Finish();
            taskProducer.CreateTask(new ChainTaskData
                {
                    ChainPosition = taskData.ChainPosition + 1,
                    ChainName = taskData.ChainName,
                    LoggingTaskIdKey = taskData.LoggingTaskIdKey
                }).Queue();
            return Finish();
        }

        private readonly IRtqTaskProducer taskProducer;
        private readonly ITestTaskLogger logger;
    }
}