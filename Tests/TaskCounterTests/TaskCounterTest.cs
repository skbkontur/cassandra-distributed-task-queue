using System;

using NUnit.Framework;

using RemoteQueue.Handling;
using RemoteQueue.Settings;

namespace RemoteQueue.Tests.TaskCounterTests
{
    [TestFixture]
    public class TaskCounterTest
    {
        [Test]
        public void TestUnlimitedAllTaskCount()
        {
            var taskCounter = CreateTaskCounter(0, 0);
            for(var i = 0; i < 100; i++)
            {
                Assert.That(taskCounter.TryIncrement(TaskQueueReason.TaskContinuation));
                Assert.That(taskCounter.TryIncrement(TaskQueueReason.PullFromQueue));
                Assert.That(taskCounter.CanQueueTask(TaskQueueReason.TaskContinuation));
                Assert.That(taskCounter.CanQueueTask(TaskQueueReason.PullFromQueue));
            }
        }

        [Test]
        public void TestUnlimitedTaskCount()
        {
            var taskCounter = CreateTaskCounter(0, 10);

            for(var i = 0; i < 100; i++)
            {
                Assert.That(taskCounter.CanQueueTask(TaskQueueReason.PullFromQueue));
                Assert.That(taskCounter.TryIncrement(TaskQueueReason.PullFromQueue));
            }

            for(var i = 0; i < 10; i++)
            {
                Assert.That(taskCounter.CanQueueTask(TaskQueueReason.TaskContinuation));
                Assert.That(taskCounter.TryIncrement(TaskQueueReason.TaskContinuation));
            }
            Assert.That(taskCounter.CanQueueTask(TaskQueueReason.TaskContinuation), Is.False);
            Assert.That(taskCounter.TryIncrement(TaskQueueReason.TaskContinuation), Is.False);

            for(var i = 0; i < 100; i++)
                taskCounter.Decrement(TaskQueueReason.PullFromQueue);

            Assert.That(taskCounter.CanQueueTask(TaskQueueReason.TaskContinuation), Is.False);
            Assert.That(taskCounter.TryIncrement(TaskQueueReason.TaskContinuation), Is.False);

            taskCounter.Decrement(TaskQueueReason.TaskContinuation);

            Assert.That(taskCounter.CanQueueTask(TaskQueueReason.TaskContinuation));
            Assert.That(taskCounter.TryIncrement(TaskQueueReason.TaskContinuation));
        }

        [Test]
        public void TestLimitedTaskCount()
        {
            var taskCounter = CreateTaskCounter(2, 3);

            Assert.That(taskCounter.CanQueueTask(TaskQueueReason.PullFromQueue));
            Assert.That(taskCounter.TryIncrement(TaskQueueReason.PullFromQueue));

            Assert.That(taskCounter.CanQueueTask(TaskQueueReason.PullFromQueue));
            Assert.That(taskCounter.TryIncrement(TaskQueueReason.PullFromQueue));

            Assert.That(taskCounter.CanQueueTask(TaskQueueReason.PullFromQueue), Is.False);
            Assert.That(taskCounter.TryIncrement(TaskQueueReason.PullFromQueue), Is.False);

            Assert.That(taskCounter.CanQueueTask(TaskQueueReason.TaskContinuation));
            Assert.That(taskCounter.TryIncrement(TaskQueueReason.TaskContinuation));

            Assert.That(taskCounter.CanQueueTask(TaskQueueReason.TaskContinuation));
            Assert.That(taskCounter.TryIncrement(TaskQueueReason.TaskContinuation));

            Assert.That(taskCounter.CanQueueTask(TaskQueueReason.TaskContinuation));
            Assert.That(taskCounter.TryIncrement(TaskQueueReason.TaskContinuation));

            Assert.That(taskCounter.CanQueueTask(TaskQueueReason.TaskContinuation), Is.False);
            Assert.That(taskCounter.TryIncrement(TaskQueueReason.TaskContinuation), Is.False);

            taskCounter.Decrement(TaskQueueReason.PullFromQueue);
            taskCounter.Decrement(TaskQueueReason.PullFromQueue);

            Assert.That(taskCounter.CanQueueTask(TaskQueueReason.PullFromQueue));
            Assert.That(taskCounter.TryIncrement(TaskQueueReason.PullFromQueue));

            Assert.That(taskCounter.CanQueueTask(TaskQueueReason.PullFromQueue));
            Assert.That(taskCounter.TryIncrement(TaskQueueReason.PullFromQueue));

            taskCounter.Decrement(TaskQueueReason.TaskContinuation);
            taskCounter.Decrement(TaskQueueReason.TaskContinuation);

            Assert.That(taskCounter.CanQueueTask(TaskQueueReason.PullFromQueue), Is.False);
            Assert.That(taskCounter.TryIncrement(TaskQueueReason.PullFromQueue), Is.False);

            Assert.That(taskCounter.CanQueueTask(TaskQueueReason.TaskContinuation));
            Assert.That(taskCounter.TryIncrement(TaskQueueReason.TaskContinuation));

            Assert.That(taskCounter.CanQueueTask(TaskQueueReason.TaskContinuation));
            Assert.That(taskCounter.TryIncrement(TaskQueueReason.TaskContinuation));
        }

        [Test]
        public void TestUnlimitedTaskContinuationCount()
        {
            var taskCounter = CreateTaskCounter(10, 0);

            for(var i = 0; i < 10; i++)
            {
                Assert.That(taskCounter.CanQueueTask(TaskQueueReason.PullFromQueue));
                Assert.That(taskCounter.TryIncrement(TaskQueueReason.PullFromQueue));
            }
            Assert.That(taskCounter.CanQueueTask(TaskQueueReason.PullFromQueue), Is.False);
            Assert.That(taskCounter.TryIncrement(TaskQueueReason.PullFromQueue), Is.False);
            taskCounter.Decrement(TaskQueueReason.PullFromQueue);
            Assert.That(taskCounter.CanQueueTask(TaskQueueReason.PullFromQueue), Is.True);
            Assert.That(taskCounter.TryIncrement(TaskQueueReason.PullFromQueue), Is.True);

            for(var i = 0; i < 100; i++)
            {
                Assert.That(taskCounter.CanQueueTask(TaskQueueReason.TaskContinuation));
                Assert.That(taskCounter.TryIncrement(TaskQueueReason.TaskContinuation));
            }

            for(var i = 0; i < 10; i++)
                taskCounter.Decrement(TaskQueueReason.PullFromQueue);

            for(var i = 0; i < 90; i++)
                taskCounter.Decrement(TaskQueueReason.TaskContinuation);

            Assert.That(taskCounter.CanQueueTask(TaskQueueReason.PullFromQueue), Is.False);
            Assert.That(taskCounter.TryIncrement(TaskQueueReason.PullFromQueue), Is.False);

            taskCounter.Decrement(TaskQueueReason.TaskContinuation);

            Assert.That(taskCounter.CanQueueTask(TaskQueueReason.PullFromQueue), Is.True);
            Assert.That(taskCounter.TryIncrement(TaskQueueReason.PullFromQueue), Is.True);
        }

        private TaskCounter CreateTaskCounter(int maxRunningTasksCount, int maxRunningContinuationsCount)
        {
            return new TaskCounter(new ExchangeSchedulableRunnerSettings(maxRunningTasksCount, maxRunningContinuationsCount));
        }

        private class ExchangeSchedulableRunnerSettings : IExchangeSchedulableRunnerSettings
        {
            public ExchangeSchedulableRunnerSettings(int maxRunningTasksCount, int maxRunningContinuationsCount)
            {
                MaxRunningContinuationsCount = maxRunningContinuationsCount;
                MaxRunningTasksCount = maxRunningTasksCount;
            }

            public TimeSpan PeriodicInterval { get; private set; }
            public int MaxRunningTasksCount { get; private set; }
            public int MaxRunningContinuationsCount { get; private set; }
            public int ShardsCount { get; private set; }
            public int ShardIndex { get; private set; }
        }
    }
}