using System;

using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas;
using SKBKontur.Catalogue.ServiceLib.Logging;

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
            var counter = testCounterRepository.DecrementCounter(Context.Id);
            if(counter == 0)
            {
                Log.For(this).InfoFormat("Finished task: {0}", Context.Id);
                return Fatal(new Exception());
            }
            var rerunInterval = Math.Min(MinDelayBeforeTaskRerunInTicks * Context.Attempts * Context.Attempts, MaxDelayBeforeTaskRerunInTicks);
            Log.For(this).InfoFormat("Rerun task: {0}, Counter: {1}", Context.Id, counter);
            return RerunAfterError(new Exception(), TimeSpan.FromTicks(rerunInterval));
        }

        private readonly long MinDelayBeforeTaskRerunInTicks = TimeSpan.FromMilliseconds(30).Ticks;
        private readonly long MaxDelayBeforeTaskRerunInTicks = TimeSpan.FromSeconds(10).Ticks;
        private readonly ITestCounterRepository testCounterRepository;
    }
}