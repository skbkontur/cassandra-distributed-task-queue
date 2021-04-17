using RemoteTaskQueue.FunctionalTests.Common.ConsumerStateImpl;
using RemoteTaskQueue.FunctionalTests.Common.TaskDatas;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace ExchangeService.UserClasses
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