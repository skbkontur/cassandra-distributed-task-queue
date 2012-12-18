using System;
using System.Linq;
using System.Threading;

using ExchangeService.Exceptions;
using ExchangeService.UserClasses;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas;

namespace FunctionalTests.ExchangeTests
{
    public class SimpleTaskTest : FunctionalTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            testCounterRepository = Container.Get<ITestCounterRepository>();
            taskQueue = Container.Get<IRemoteTaskQueue>();
        }

        [Test]
        public void TestRun()
        {
            var taskId = taskQueue.Queue(new SimpleTaskData());
            Wait(new []{taskId}, 1);
            Thread.Sleep(2000);
            Assert.AreEqual(1, testCounterRepository.GetCounter(taskId));
            Assert.AreEqual(TaskState.Finished, taskQueue.GetTaskInfo<SimpleTaskData>(taskId).Context.State);
        }

        [Test]
        public void TestCancel()
        {
            var taskId = taskQueue.Queue(new SimpleTaskData(), TimeSpan.FromSeconds(1));
            taskQueue.CancelTask(taskId);
            Wait(new[] { taskId }, 0);
            Thread.Sleep(2000);
            Assert.AreEqual(0, testCounterRepository.GetCounter(taskId));
            Assert.AreEqual(TaskState.Canceled, taskQueue.GetTaskInfo<SimpleTaskData>(taskId).Context.State);
        }

        [Test]
        public void TestRerun()
        {
            var taskId = taskQueue.Queue(new SimpleTaskData());
            Wait(new[] { taskId }, 1);
            taskQueue.RerunTask(taskId, TimeSpan.FromMilliseconds(1));
            Wait(new[] { taskId }, 2);
            Thread.Sleep(2000);
            Assert.AreEqual(2, testCounterRepository.GetCounter(taskId));
            Assert.AreEqual(TaskState.Finished, taskQueue.GetTaskInfo<SimpleTaskData>(taskId).Context.State);
            Assert.AreEqual(2, taskQueue.GetTaskInfo<SimpleTaskData>(taskId).Context.Attempts);
        }

        private void Wait(string[] taskIds, int criticalValue, int ms = 5000)
        {
            int current = 0;
            while (true)
            {
                var attempts = taskIds.Select(testCounterRepository.GetCounter).ToArray();
                Console.WriteLine(Now() + " CurrentValues: " + String.Join(", ", attempts));
                int minValue = attempts.Min();
                if (minValue >= criticalValue)
                    break;
                Thread.Sleep(sleepInterval);
                current += sleepInterval;
                if (current > ms)
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