using System;

using RemoteQueue.Handling;
using RemoteQueue.Handling.HandlerResults;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas;

namespace ExchangeService.UserClasses
{
    public class SimpleTaskHandler : TaskHandler<SimpleTaskData>
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