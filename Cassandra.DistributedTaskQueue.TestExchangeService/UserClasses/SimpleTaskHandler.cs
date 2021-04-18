using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.ConsumerStateImpl;
using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.TaskDatas;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.TestExchangeService.UserClasses
{
    public class SimpleTaskHandler : RtqTaskHandler<SimpleTaskData>
    {
        public SimpleTaskHandler(ITestCounterRepository testCounterRepository)
        {
            this.testCounterRepository = testCounterRepository;
        }

        protected override HandleResult HandleTask(SimpleTaskData taskData)
        {
            testCounterRepository.IncrementCounter(Context.Id);
            return new HandleResult
                {
                    FinishAction = FinishAction.Finish
                };
        }

        private readonly ITestCounterRepository testCounterRepository;
    }
}