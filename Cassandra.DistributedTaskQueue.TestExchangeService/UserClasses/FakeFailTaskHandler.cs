using System;

using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common;
using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.ConsumerStateImpl;
using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.TaskDatas;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.TestExchangeService.UserClasses
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
                Log.For(this).Info("Finished task: {RtqTaskId}", new {RtqTaskId = Context.Id});
                return Fatal(new Exception());
            }
            var rerunInterval = TimeSpan.FromTicks(Math.Min(minDelayBeforeTaskRerun.Ticks * Context.Attempts * Context.Attempts, maxDelayBeforeTaskRerun.Ticks));
            Log.For(this).Info("Rerun task: {RtqTaskId}, Counter: {counter}, RerunInterval: {rerunInterval}",
                               new {RtqTaskId = Context.Id, counter, rerunInterval});
            return RerunAfterError(new Exception(), rerunInterval);
        }

        private readonly TimeSpan minDelayBeforeTaskRerun = TimeSpan.FromMilliseconds(30);
        private readonly TimeSpan maxDelayBeforeTaskRerun = TimeSpan.FromSeconds(10);
        private readonly ITestCounterRepository testCounterRepository;
    }
}