using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.ConsumerStateImpl;
using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.TaskDatas;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.TestExchangeService.UserClasses
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