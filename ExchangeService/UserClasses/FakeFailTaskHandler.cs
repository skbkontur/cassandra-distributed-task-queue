using System;

using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas;

namespace ExchangeService.UserClasses
{
    public class FakeFailTaskHandler : TaskHandler<FakeFailTaskData>
    {
        public FakeFailTaskHandler(ITestCounterRepository testCounterRepository)
        {
            this.testCounterRepository = testCounterRepository;
        }

        protected override HandleResult HandleTask(FakeFailTaskData taskData)
        {
            var decrementCounter = testCounterRepository.DecrementCounter(Context.Id);
            if(decrementCounter == 0)
                return Fatal(new Exception());
            var rerunInterval = Math.Min(MinDelayBeforeTaskRerunInTicks * Context.Attempts * Context.Attempts, MaxDelayBeforeTaskRerunInTicks);
            return RerunAfterError(new Exception(), TimeSpan.FromTicks(rerunInterval));
        }

        private readonly long MinDelayBeforeTaskRerunInTicks = TimeSpan.FromMilliseconds(30).Ticks;
        private readonly long MaxDelayBeforeTaskRerunInTicks = TimeSpan.FromSeconds(10).Ticks;
        private readonly ITestCounterRepository testCounterRepository;
    }
}