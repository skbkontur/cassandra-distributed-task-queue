using System;
using System.IO;

using ExchangeService.Repositories;
using ExchangeService.TaskDatas;

using RemoteQueue.Handling;
using RemoteQueue.Handling.HandlerResults;

namespace ExchangeService.TaskHandlers
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
            Log(decrementCounter.ToString() + "\n");
            if(decrementCounter == 0)
            {
                Log("Finish\n");
                return Finish();
            }
            var now = DateTime.UtcNow.Ticks;
            var rerunInterval = GetMinimalStartTicks(Context.Attempts, now) - now;
            Log("Rerun\n");
            return Rerun(TimeSpan.FromTicks(rerunInterval));
        }

        private long GetMinimalStartTicks(int attempts, long nowTicks)
        {
            return nowTicks + Math.Min(MinDelayBeforeTaskRerunInTicks * attempts * attempts, MaxDelayBeforeTaskRerunInTicks);
        }

        private void Log(string text)
        {
//            File.AppendAllText(@"c:\logs\" + Context.Id + ".txt", text);
        }

        private readonly long MinDelayBeforeTaskRerunInTicks = TimeSpan.FromSeconds(0.1).Ticks;
        private readonly long MaxDelayBeforeTaskRerunInTicks = TimeSpan.FromMinutes(30).Ticks;
        private readonly ITestCounterRepository testCounterRepository;
    }
}