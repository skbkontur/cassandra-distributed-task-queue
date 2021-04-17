using System;

using NUnit.Framework;

using RemoteTaskQueue.FunctionalTests.Common.TaskDatas;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests.RemoteTaskQueue.ExchangeTests
{
    public class PeriodicTaskTest : TasksWithCounterTestBase
    {
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
            for (var i = 0; i < count; i++)
                keys[i] = AddTask(3);
            WaitForFinishedState(keys);
        }

        [Test]
        public void TestManyPeriodicTaskWithTaskGroupLock()
        {
            const int count = 10;
            var keys = new string[count];
            for (var i = 0; i < count; i++)
                keys[i] = AddTask(3, "Lock" + (i % 3));
            WaitForFinishedState(keys);
        }

        private string AddTask(int attempts, string taskGroupLock = null)
        {
            var task = remoteTaskQueue.CreateTask(new FakePeriodicTaskData(),
                                                  new CreateTaskOptions
                                                      {
                                                          TaskGroupLock = taskGroupLock
                                                      });
            testCounterRepository.SetValueForCounter(task.Id, attempts);
            task.Queue();
            return task.Id;
        }

        private void WaitForFinishedState(string[] taskIds)
        {
            WaitForTerminalState(taskIds, TaskState.Finished, "FakePeriodicTaskData", TimeSpan.FromSeconds(5));
        }
    }
}