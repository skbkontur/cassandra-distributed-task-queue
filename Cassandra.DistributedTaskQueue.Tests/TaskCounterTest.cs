using NUnit.Framework;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests
{
    [TestFixture]
    public class TaskCounterTest
    {
        [Test]
        public void TestUnlimitedAllTaskCount()
        {
            var taskCounter = new LocalQueueTaskCounter(0, 0);
            for (var i = 0; i < 100; i++)
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
            var taskCounter = new LocalQueueTaskCounter(0, 10);

            for (var i = 0; i < 100; i++)
            {
                Assert.That(taskCounter.CanQueueTask(TaskQueueReason.PullFromQueue));
                Assert.That(taskCounter.TryIncrement(TaskQueueReason.PullFromQueue));
            }

            for (var i = 0; i < 10; i++)
            {
                Assert.That(taskCounter.CanQueueTask(TaskQueueReason.TaskContinuation));
                Assert.That(taskCounter.TryIncrement(TaskQueueReason.TaskContinuation));
            }
            Assert.That(taskCounter.CanQueueTask(TaskQueueReason.TaskContinuation), Is.False);
            Assert.That(taskCounter.TryIncrement(TaskQueueReason.TaskContinuation), Is.False);

            for (var i = 0; i < 100; i++)
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
            var taskCounter = new LocalQueueTaskCounter(2, 3);

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
            var taskCounter = new LocalQueueTaskCounter(10, 0);

            for (var i = 0; i < 10; i++)
            {
                Assert.That(taskCounter.CanQueueTask(TaskQueueReason.PullFromQueue));
                Assert.That(taskCounter.TryIncrement(TaskQueueReason.PullFromQueue));
            }
            Assert.That(taskCounter.CanQueueTask(TaskQueueReason.PullFromQueue), Is.False);
            Assert.That(taskCounter.TryIncrement(TaskQueueReason.PullFromQueue), Is.False);
            taskCounter.Decrement(TaskQueueReason.PullFromQueue);
            Assert.That(taskCounter.CanQueueTask(TaskQueueReason.PullFromQueue), Is.True);
            Assert.That(taskCounter.TryIncrement(TaskQueueReason.PullFromQueue), Is.True);

            for (var i = 0; i < 100; i++)
            {
                Assert.That(taskCounter.CanQueueTask(TaskQueueReason.TaskContinuation));
                Assert.That(taskCounter.TryIncrement(TaskQueueReason.TaskContinuation));
            }

            for (var i = 0; i < 10; i++)
                taskCounter.Decrement(TaskQueueReason.PullFromQueue);

            for (var i = 0; i < 90; i++)
                taskCounter.Decrement(TaskQueueReason.TaskContinuation);

            Assert.That(taskCounter.CanQueueTask(TaskQueueReason.PullFromQueue), Is.False);
            Assert.That(taskCounter.TryIncrement(TaskQueueReason.PullFromQueue), Is.False);

            taskCounter.Decrement(TaskQueueReason.TaskContinuation);

            Assert.That(taskCounter.CanQueueTask(TaskQueueReason.PullFromQueue), Is.True);
            Assert.That(taskCounter.TryIncrement(TaskQueueReason.PullFromQueue), Is.True);
        }
    }
}