using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using GroboContainer.NUnitExtensions;

using NUnit.Framework;

using RemoteTaskQueue.FunctionalTests.Common;
using RemoteTaskQueue.FunctionalTests.Common.ConsumerStateImpl;
using RemoteTaskQueue.FunctionalTests.Common.TaskDatas;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.TimeBasedUuid;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests.RemoteTaskQueue.ExchangeTests
{
    public class SimpleTaskTest : ExchangeTestBase
    {
        [Test]
        [Repeat(10)]
        public void TestRun()
        {
            var taskId = remoteTaskQueue.CreateTask(new SimpleTaskData()).Queue();
            Wait(new[] {taskId}, 1);
            Thread.Sleep(2000);
            Assert.AreEqual(1, testCounterRepository.GetCounter(taskId));
            Assert.AreEqual(TaskState.Finished, remoteTaskQueue.GetTaskInfo<SimpleTaskData>(taskId).Context.State);
            CheckTaskMinimalStartTicksIndexStates(new Dictionary<string, TaskIndexShardKey>
                {
                    {taskId, TaskIndexShardKey("SimpleTaskData", TaskState.Finished)}
                });
        }

        [Test]
        public void TestRunMultipleTasks()
        {
            var taskIds = new List<string>();
            Enumerable.Range(0, 42).AsParallel().ForAll(x =>
                {
                    var taskId = remoteTaskQueue.CreateTask(new SimpleTaskData()).Queue();
                    lock (taskIds)
                        taskIds.Add(taskId);
                });
            WaitForTasksToFinish(taskIds, TimeSpan.FromSeconds(10));
            CheckTaskMinimalStartTicksIndexStates(taskIds.ToDictionary(s => s, s => TaskIndexShardKey("SimpleTaskData", TaskState.Finished)));
        }

        [Test]
        public void TestCancel()
        {
            var taskId = remoteTaskQueue.CreateTask(new SimpleTaskData()).Queue(TimeSpan.FromSeconds(1));
            Assert.That(remoteTaskQueue.TryCancelTask(taskId), Is.EqualTo(TaskManipulationResult.Success));
            Wait(new[] {taskId}, 0);
            Thread.Sleep(2000);
            Assert.AreEqual(0, testCounterRepository.GetCounter(taskId));
            Assert.AreEqual(TaskState.Canceled, remoteTaskQueue.GetTaskInfo<SimpleTaskData>(taskId).Context.State);
            CheckTaskMinimalStartTicksIndexStates(new Dictionary<string, TaskIndexShardKey>
                {
                    {taskId, TaskIndexShardKey("SimpleTaskData", TaskState.Canceled)}
                });
        }

        [Test]
        public void TestCancel_UnknownTask()
        {
            var taskId = TimeGuid.NowGuid().ToGuid().ToString();
            Assert.That(remoteTaskQueue.TryCancelTask(taskId), Is.EqualTo(TaskManipulationResult.Failure_TaskDoesNotExist));
        }

        [Test]
        public void TestCancel_LockAcquiringFails()
        {
            var taskId = remoteTaskQueue.CreateTask(new SimpleTaskData()).Queue(TimeSpan.FromSeconds(5));
            var remoteLockCreator = remoteTaskQueue.RemoteLockCreator;
            using (remoteLockCreator.Lock(taskId))
                Assert.That(remoteTaskQueue.TryCancelTask(taskId), Is.EqualTo(TaskManipulationResult.Failure_LockAcquiringFails));
        }

        [Test]
        public void TestRerun()
        {
            var taskId = remoteTaskQueue.CreateTask(new SimpleTaskData()).Queue();
            Wait(new[] {taskId}, 1);
            Assert.That(remoteTaskQueue.TryRerunTask(taskId, TimeSpan.FromMilliseconds(1)), Is.EqualTo(TaskManipulationResult.Success));
            Wait(new[] {taskId}, 2);
            Thread.Sleep(2000);
            Assert.AreEqual(2, testCounterRepository.GetCounter(taskId));
            var taskMeta = remoteTaskQueue.GetTaskInfo<SimpleTaskData>(taskId).Context;
            Assert.AreEqual(TaskState.Finished, taskMeta.State);
            Assert.AreEqual(2, taskMeta.Attempts);
            CheckTaskMinimalStartTicksIndexStates(new Dictionary<string, TaskIndexShardKey>
                {
                    {taskId, TaskIndexShardKey("SimpleTaskData", TaskState.Finished)}
                });
        }

        [Test]
        public void TestRerun_UnknownTask()
        {
            var taskId = TimeGuid.NowGuid().ToGuid().ToString();
            Assert.That(remoteTaskQueue.TryRerunTask(taskId, TimeSpan.Zero), Is.EqualTo(TaskManipulationResult.Failure_TaskDoesNotExist));
        }

        [Test]
        public void TestRerun_LockAcquiringFails()
        {
            var taskId = remoteTaskQueue.CreateTask(new SimpleTaskData()).Queue(TimeSpan.FromSeconds(5));
            using (remoteTaskQueue.RemoteLockCreator.Lock(taskId))
                Assert.That(remoteTaskQueue.TryRerunTask(taskId, TimeSpan.Zero), Is.EqualTo(TaskManipulationResult.Failure_LockAcquiringFails));
        }

        private void Wait(string[] taskIds, int criticalValue, int ms = 5000)
        {
            var current = 0;
            while (true)
            {
                var attempts = taskIds.Select(testCounterRepository.GetCounter).ToArray();
                Log.For(this).Info("{Now} CurrentValues: {CurrentValues}", new {Now = Now(), CurrentValues = string.Join(", ", attempts)});
                var minValue = attempts.Min();
                if (minValue >= criticalValue)
                    break;
                Thread.Sleep(sleepInterval);
                current += sleepInterval;
                if (current > ms)
                    throw new TooLateException("Время ожидания превысило {0} мс.", ms);
            }
        }

        private void WaitForTasksToFinish(IEnumerable<string> taskIds, TimeSpan timeSpan)
        {
            Assert.That(() =>
                            {
                                var tasks = remoteTaskQueue.HandleTaskCollection.GetTasks(taskIds.ToArray());
                                return tasks.All(t => t.Meta.State == TaskState.Finished);
                            },
                        Is.True.After((int)timeSpan.TotalMilliseconds, 100));
        }

        private static string Now()
        {
            return DateTime.UtcNow.ToString("dd.MM.yyyy mm:hh:ss.ffff");
        }

        private const int sleepInterval = 200;

        [Injected]
        private ITestCounterRepository testCounterRepository;
    }
}