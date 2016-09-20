using System;

using ExchangeService.Exceptions;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas;

namespace FunctionalTests.ExchangeTests
{
    public class FailTaskTest : TasksWithCounterTestBase
    {
        [Test]
        public void TestTooLateOneFailTask()
        {
            var taskId = AddTask(9);
            try
            {
                WaitForFatalState(new[] {taskId}, TimeSpan.FromSeconds(1));
                throw new Exception("Wait не должен отработать");
            }
            catch(TooLateException)
            {
                var count = testCounterRepository.GetCounter(taskId);
                Assert.That(count < 9 && count >= 1);
            }
        }

        [Test]
        public void TestManyFails()
        {
            const int count = 5;
            var ids = new string[count];
            for(var i = 0; i < count; i++)
                ids[i] = AddTask(7);
            WaitForFatalState(ids, TimeSpan.FromSeconds(60));
        }

        [Test]
        [Repeat(10)]
        public void TestOneFailTask()
        {
            var taskId = AddTask(3);
            WaitForFatalState(new[] {taskId}, TimeSpan.FromSeconds(5));
        }

        [Test]
        public void TestManyFailTask()
        {
            const int count = 2;
            var ids = new string[count];
            for(var i = 0; i < count; i++)
                ids[i] = AddTask(3);
            WaitForFatalState(ids, TimeSpan.FromSeconds(15));
        }

        [Test]
        [Repeat(10)]
        public void TestStressManyTasks()
        {
            const int count = 100;
            var ids = new string[count];
            for(var j = 0; j < count; j++)
                ids[j] = AddTask(10);
            WaitForFatalState(ids, TimeSpan.FromSeconds(90));
        }

        private string AddTask(int attempts)
        {
            var task = taskQueue.CreateTask(new FakeFailTaskData());
            testCounterRepository.SetValueForCounter(task.Id, attempts);
            task.Queue();
            return task.Id;
        }

        private void WaitForFatalState(string[] taskIds, TimeSpan timeout)
        {
            WaitForTerminalState(taskIds, TaskState.Fatal, "FakeFailTaskData", timeout);
        }
    }
}