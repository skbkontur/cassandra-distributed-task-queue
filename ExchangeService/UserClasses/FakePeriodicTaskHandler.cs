using System;

using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas;

namespace ExchangeService.UserClasses
{
    public class FakePeriodicTaskHandler : TaskHandler<FakePeriodicTaskData>
    {
        public FakePeriodicTaskHandler(ITestCounterRepository testCounterRepository)
        {
            this.testCounterRepository = testCounterRepository;
        }

        protected override HandleResult HandleTask(FakePeriodicTaskData taskData)
        {
            var decrementCounter = testCounterRepository.DecrementCounter(Context.Id);
            if(decrementCounter == 0)
                return Finish();
            return Rerun(TimeSpan.FromMilliseconds(100));
        }

        private readonly ITestCounterRepository testCounterRepository;
    }
}