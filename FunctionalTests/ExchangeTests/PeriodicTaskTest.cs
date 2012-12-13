using System;
using System.Linq;
using System.Threading;

using ExchangeService.Exceptions;
using ExchangeService.UserClasses;

using NUnit.Framework;

using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas;

namespace FunctionalTests.ExchangeTests
{
    public class PeriodicTaskTest : FunctionalTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            testCounterRepository = Container.Get<ITestCounterRepository>();
            taskQueue = Container.Get<IRemoteTaskQueue>();
        }

        [Test]
        public void TestOnePeriodicTask()
        {
            var taskId = AddTask();
            Wait(new[] {taskId}, 10);
        }

        [Test]
        public void TestManyPeriodicTask()
        {
            const int count = 10;
            var keys = new string[count];
            for(int i = 0; i < count; i++)
                keys[i] = AddTask();
            Wait(keys, 10);
        }

        private string AddTask()
        {
            return taskQueue.Queue(new FakePeriodicTaskData());
        }

        private void Wait(string[] taskIds, int criticalValue, int ms = 5000)
        {
            int current = 0;
            while(true)
            {
                var attempts = taskIds.Select(testCounterRepository.GetCounter).ToArray();
                Console.WriteLine(Now() + " CurrentValues: " + String.Join(", ", attempts));
                int minValue = attempts.Min();
                if(minValue >= criticalValue)
                    break;
                Thread.Sleep(sleepInterval);
                current += sleepInterval;
                if(current > ms)
                    throw new TooLateException("Время ожидания превысило {0} мс.", ms);
            }
        }

        private static string Now()
        {
            return DateTime.UtcNow.ToString("dd.MM.yyyy mm:hh:ss.ffff");
        }

        private ITestCounterRepository testCounterRepository;
        private IRemoteTaskQueue taskQueue;
        private const int sleepInterval = 200;
    }
}