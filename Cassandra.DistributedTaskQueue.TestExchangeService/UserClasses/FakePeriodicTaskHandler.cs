using System;

using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.ConsumerStateImpl;
using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.TaskDatas;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.TestExchangeService.UserClasses
{
    public class FakePeriodicTaskHandler : RtqTaskHandler<FakePeriodicTaskData>
    {
        public FakePeriodicTaskHandler(ITestCounterRepository testCounterRepository)
        {
            this.testCounterRepository = testCounterRepository;
        }

        protected override HandleResult HandleTask(FakePeriodicTaskData taskData)
        {
            var decrementCounter = testCounterRepository.DecrementCounter(Context.Id);
            if (decrementCounter == 0)
                return Finish();
            return Rerun(TimeSpan.FromMilliseconds(100));
        }

        private readonly ITestCounterRepository testCounterRepository;
    }
}