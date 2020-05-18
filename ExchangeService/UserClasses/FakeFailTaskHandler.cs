using System;

using RemoteTaskQueue.FunctionalTests.Common.ConsumerStateImpl;
using RemoteTaskQueue.FunctionalTests.Common.TaskDatas;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

using SKBKontur.Catalogue.ServiceLib.Logging;

using Vostok.Logging.Abstractions;

namespace ExchangeService.UserClasses
{
    public class FakeFailTaskHandler : RtqTaskHandler<FakeFailTaskData>
    {
        public FakeFailTaskHandler(ITestCounterRepository testCounterRepository)
        {
            this.testCounterRepository = testCounterRepository;
        }

        protected override HandleResult HandleTask(FakeFailTaskData taskData)
        {
            var counter = testCounterRepository.DecrementCounter(Context.Id);
            if (counter == 0)
            {
                Log.For(this).Info("Finished task: {ContextId}", new {ContextId = Context.Id});
                return Fatal(new Exception());
            }
            var rerunInterval = TimeSpan.FromTicks(Math.Min(minDelayBeforeTaskRerun.Ticks * Context.Attempts * Context.Attempts, maxDelayBeforeTaskRerun.Ticks));
            Log.For(this).Info("Rerun task: {ContextId}, Counter: {Counter}, RerunInterval: {RerunInterval}",
                               new {ContextId = Context.Id, Counter = counter, RerunInterval = rerunInterval});
            return RerunAfterError(new Exception(), rerunInterval);
        }

        private readonly TimeSpan minDelayBeforeTaskRerun = TimeSpan.FromMilliseconds(30);
        private readonly TimeSpan maxDelayBeforeTaskRerun = TimeSpan.FromSeconds(10);
        private readonly ITestCounterRepository testCounterRepository;
    }
}