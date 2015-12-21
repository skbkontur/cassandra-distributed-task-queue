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
            var rerunInterval = TimeSpan.FromTicks(Math.Min(minDelayBeforeTaskRerun.Ticks * Context.Attempts * Context.Attempts, maxDelayBeforeTaskRerun.Ticks));
            Log.For(this).InfoFormat("Rerun task: {0}, Counter: {1}, RerunInterval: {2}", Context.Id, counter, rerunInterval);
            return RerunAfterError(new Exception(), rerunInterval);
        }

        private readonly TimeSpan minDelayBeforeTaskRerun = TimeSpan.FromMilliseconds(30);
        private readonly TimeSpan maxDelayBeforeTaskRerun = TimeSpan.FromSeconds(10);
        private readonly ITestCounterRepository testCounterRepository;
    }
}