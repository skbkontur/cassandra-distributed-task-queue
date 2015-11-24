using System.Threading;

using RemoteQueue.Handling;
using RemoteQueue.Handling.HandlerResults;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;

namespace ExchangeService.UserClasses.MonitoringTestTaskData
{
    public class SlowTaskHandler : TaskHandler<SlowTaskData>
    {
        public SlowTaskHandler(ITestCounterRepository testCounterRepository)
        {
            this.testCounterRepository = testCounterRepository;
        }

        protected override HandleResult HandleTask(SlowTaskData taskData)
        {
            if(taskData.UseCounter)
                testCounterRepository.IncrementCounter("SlowTaskHandler_Started");

            Thread.Sleep(taskData.TimeMs);
            var handleResult = Finish();

            if(taskData.UseCounter)
                testCounterRepository.IncrementCounter("SlowTaskHandler_Finished");
            return handleResult;
        }

        private readonly ITestCounterRepository testCounterRepository;
    }
}