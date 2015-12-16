using System;
using System.Linq;
using System.Threading;

using ExchangeService.Exceptions;
using ExchangeService.UserClasses;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
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
            handleTaskCollection = Container.Get<IHandleTaskCollection>();
        }

        [Test]
        public void TestOnePeriodicTask()
        {
            var taskId = AddTask(3);
            Wait(new[] {taskId});
        }

        [Test]
        public void TestManyPeriodicTask()
        {
            const int count = 10;
            var keys = new string[count];
            for(var i = 0; i < count; i++)
                keys[i] = AddTask(3);
            Wait(keys);
        }

        [Test]
        public void TestManyPeriodicTaskWithTaskGroupLock()
        {
            const int count = 10;
            var keys = new string[count];
            for(var i = 0; i < count; i++)
                keys[i] = AddTask(3, "Lock" + (i % 3));
            Wait(keys);
        }

        private string AddTask(int attempts, string taskGroupLock = null)
        {
            var task = taskQueue.CreateTask(new FakePeriodicTaskData(), new CreateTaskOptions
                {
                    TaskGroupLock = taskGroupLock
                });
            testCounterRepository.SetValueForCounter(task.Id, attempts);
            task.Queue();
            return task.Id;
        }

        private void Wait(string[] taskIds, int ms = 15000)
        {
            var current = 0;
            while(true)
            {
                var running = false;
                for(var i = 0; i < taskIds.Length; i++)
                {
                    var task = handleTaskCollection.GetTask(taskIds[i]);
                    if(task.Meta.State != TaskState.Finished)
                        running = true;
                }

                var attempts = taskIds.Select(testCounterRepository.GetCounter).ToArray();
                Console.WriteLine(Now() + " CurrentValues: " + string.Join(", ", attempts));
                if(!running)
                {
                    for(var i = 0; i < attempts.Length; i++)
                    {
                        var attempt = attempts[i];
                        if(attempt != 0)
                            Console.WriteLine(taskIds[i]);
                        Assert.AreEqual(0, attempt);
                    }
                    Container.CheckTaskMinimalStartTicksIndexStates(taskIds.ToDictionary(s => s, s => TaskIndexShardKey("FakePeriodicTaskData", TaskState.Finished)));
                    break;
                }

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

        private const int sleepInterval = 200;

        private ITestCounterRepository testCounterRepository;
        private IRemoteTaskQueue taskQueue;
        private IHandleTaskCollection handleTaskCollection;
    }
}