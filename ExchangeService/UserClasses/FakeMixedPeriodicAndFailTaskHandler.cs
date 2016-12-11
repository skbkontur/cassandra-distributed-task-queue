using System;
using System.Linq;

using RemoteQueue.Handling;

using RemoteTaskQueue.FunctionalTests.Common.TaskDatas;

namespace ExchangeService.UserClasses
{
    public class FakeMixedPeriodicAndFailTaskHandler : TaskHandler<FakeMixedPeriodicAndFailTaskData>
    {
        public FakeMixedPeriodicAndFailTaskHandler(ITestCounterRepository testCounterRepository)
        {
            this.testCounterRepository = testCounterRepository;
        }

        protected override HandleResult HandleTask(FakeMixedPeriodicAndFailTaskData taskData)
        {
            var decrementCounter = testCounterRepository.DecrementCounter(Context.Id);
            if(decrementCounter == 0)
                return Finish();
            if(taskData.FailCounterValues != null && taskData.FailCounterValues.Contains(decrementCounter + 1))
                return RerunAfterError(new Exception("Exception-" + Guid.NewGuid()), taskData.RerunAfter);
            else
                return Rerun(taskData.RerunAfter);
        }

        private readonly ITestCounterRepository testCounterRepository;
    }
}