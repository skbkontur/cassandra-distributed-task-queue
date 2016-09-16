using System;

using FunctionalTests.RepositoriesTests;

using GroBuf;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Configuration;
using RemoteQueue.Handling;
using RemoteQueue.Profiling;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas;

namespace FunctionalTests.ExchangeTests
{
    public class TaskProlongationTest : TasksWithCounterTestBase
    {
        [Test]
        public void Prolongation_Happened()
        {
            var estimatedNumberOfRuns = 2 * (int)(tasksTtl.Ticks / FakePeriodicTaskData.RerunAfter.Ticks);
            Console.WriteLine("Estimated number of runs: {0}", estimatedNumberOfRuns);
            var taskId = AddTask(estimatedNumberOfRuns);
            Wait(new[] {taskId}, TaskState.Finished, "FakePeriodicTaskData", TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(100));
            Assert.That(taskQueue.GetTaskInfo<FakePeriodicTaskData>(taskId).Context.TtlTicks, Is.GreaterThan(tasksTtl.Ticks));
        }

        [Test]
        public void Prolongation_NotHappened()
        {
            var taskId = AddTask(3);
            Wait(new[] {taskId}, TaskState.Finished, "FakePeriodicTaskData", TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(100));
            Assert.That(() => taskQueue.GetTaskInfos<FakePeriodicTaskData>(new[] {taskId}).Length, Is.EqualTo(0).After(10000, 100));
        }

        private string AddTask(int attempts)
        {
            var task = taskQueue.CreateTask(new FakePeriodicTaskData());
            testCounterRepository.SetValueForCounter(task.Id, attempts);
            task.Queue();
            return task.Id;
        }

        protected override IRemoteTaskQueue GetRemoteTaskQueue()
        {
            return new RemoteTaskQueue(
                Container.Get<ISerializer>(),
                Container.Get<ICassandraCluster>(),
                new SmallTtlRemoteTaskQueueSettings(Container.Get<IRemoteTaskQueueSettings>(), tasksTtl),
                Container.Get<ITaskDataRegistry>(),
                Container.Get<IRemoteTaskQueueProfiler>()
                );
        }

        private readonly TimeSpan tasksTtl = TimeSpan.FromSeconds(2);
    }
}