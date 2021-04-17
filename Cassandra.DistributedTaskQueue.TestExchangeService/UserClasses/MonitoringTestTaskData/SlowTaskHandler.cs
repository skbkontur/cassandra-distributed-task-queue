using System.Threading;

using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.ConsumerStateImpl;
using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.TestExchangeService.UserClasses.MonitoringTestTaskData
{
    public class SlowTaskHandler : RtqTaskHandler<SlowTaskData>
    {
        public SlowTaskHandler(ITestCounterRepository testCounterRepository)
        {
            this.testCounterRepository = testCounterRepository;
        }

        protected override HandleResult HandleTask(SlowTaskData taskData)
        {
            if (taskData.UseCounter)
                testCounterRepository.IncrementCounter("SlowTaskHandler_Started");

            Thread.Sleep(taskData.TimeMs);
            var handleResult = Finish();

            if (taskData.UseCounter)
                testCounterRepository.IncrementCounter("SlowTaskHandler_Finished");
            return handleResult;
        }

        private readonly ITestCounterRepository testCounterRepository;
    }
}