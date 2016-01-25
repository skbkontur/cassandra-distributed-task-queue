using System;

using ExchangeService.UserClasses;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas;

namespace FunctionalTests.ExchangeTests
{
    public class PeriodicTaskTest : TasksWithCounterTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            testCounterRepository = Container.Get<ITestCounterRepository>();
            taskQueue = Container.Get<IRemoteTaskQueue>();
            handleTaskCollection = Container.Get<IHandleTaskCollection>();
        }

        [Test]
        [Repeat(10)]
        public void TestOnePeriodicTask()
        {
            var taskId = AddTask(3);
            WaitForFinishedState(new[] {taskId});
        }

        [Test]
        public void TestManyPeriodicTask()
        {
            const int count = 10;
            var keys = new string[count];
            for(var i = 0; i < count; i++)
                keys[i] = AddTask(3);
            WaitForFinishedState(keys);
        }

        [Test]
        public void TestManyPeriodicTaskWithTaskGroupLock()
        {
            const int count = 10;
            var keys = new string[count];
            for(var i = 0; i < count; i++)
                keys[i] = AddTask(3, "Lock" + (i % 3));
            WaitForFinishedState(keys);
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

        private void WaitForFinishedState(string[] taskIds)
        {
            Wait(taskIds, TaskState.Finished, "FakePeriodicTaskData", TimeSpan.FromSeconds(5));
        }

        private IRemoteTaskQueue taskQueue;
    }
}