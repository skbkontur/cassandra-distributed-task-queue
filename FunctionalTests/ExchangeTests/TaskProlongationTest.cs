using System;
using System.Threading;

using FunctionalTests.RepositoriesTests;

using GroBuf;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
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
        public override void SetUp()
        {
            base.SetUp();
            taskDataStorage = Container.Get<ITaskDataStorage>();
            taskMetaStorage = Container.Get<ITaskMetaStorage>();
            taskExceptionInfoStorage = Container.Get<ITaskExceptionInfoStorage>();
        }

        [Test]
        public void Prolongation_Happened()
        {
            var rerunAfter = TimeSpan.FromMilliseconds(100);

            var estimatedNumberOfRuns = 2 * (int)(tasksTtl.Ticks / rerunAfter.Ticks);
            Console.WriteLine("Estimated number of runs: {0}", estimatedNumberOfRuns);
            var taskId = AddTask(estimatedNumberOfRuns, rerunAfter, new[] {estimatedNumberOfRuns, estimatedNumberOfRuns - 1, estimatedNumberOfRuns - 2});
            WaitForTerminalState(new[] {taskId}, TaskState.Finished, "FakeMixedPeriodicAndFailTaskData", TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(100));
            Assert.That(taskQueue.GetTaskInfo<FakeMixedPeriodicAndFailTaskData>(taskId).Context.TtlTicks, Is.GreaterThan(tasksTtl.Ticks));

            Thread.Sleep(10000);
            var taskInfo = taskQueue.GetTaskInfo<FakeMixedPeriodicAndFailTaskData>(taskId);
            Assert.That(taskInfo.ExceptionInfos.Length, Is.EqualTo(3));
        }

        [Test]
        public void Prolongation_NotHappened()
        {
            var taskId = AddTask(2, TimeSpan.FromMilliseconds(100), new[] {2});
            WaitForTerminalState(new[] {taskId}, TaskState.Finished, "FakeMixedPeriodicAndFailTaskData", TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(100));
            var meta = taskQueue.GetTaskInfo<FakeMixedPeriodicAndFailTaskData>(taskId).Context;

            Assert.That(() => taskQueue.GetTaskInfos<FakeMixedPeriodicAndFailTaskData>(new[] {taskId}).Length, Is.EqualTo(0).After(10000, 100));
            Assert.That(() => taskMetaStorage.Read(taskId), Is.Null.After(10000, 100));
            Assert.That(() => taskDataStorage.Read(meta), Is.Null.After(10000, 100));
            Assert.That(() => taskExceptionInfoStorage.Read(new[] {meta})[meta.Id].Length, Is.EqualTo(0).After(10000, 100));
        }

        [Test]
        public void Prolongation_FarRerunProtection()
        {
            var bigRerunPeriod = TimeSpan.FromDays(1440);
            var taskId = AddTask(2, bigRerunPeriod, new[] {2});
            WaitForState(new[] {taskId}, TaskState.WaitingForRerunAfterError, TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(100));
            Assert.That(taskQueue.GetTaskInfo<FakeMixedPeriodicAndFailTaskData>(taskId).Context.TtlTicks, Is.GreaterThanOrEqualTo(bigRerunPeriod.Ticks));

            Thread.Sleep(10000);
            var taskInfo = taskQueue.GetTaskInfo<FakeMixedPeriodicAndFailTaskData>(taskId);
            Assert.That(taskInfo.ExceptionInfos.Length, Is.EqualTo(1));
        }

        private string AddTask(int attempts, TimeSpan rerunAfter, int[] failCounterValues)
        {
            var task = taskQueue.CreateTask(new FakeMixedPeriodicAndFailTaskData(rerunAfter, failCounterValues));
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

        private ITaskExceptionInfoStorage taskExceptionInfoStorage;
        private ITaskDataStorage taskDataStorage;
        private ITaskMetaStorage taskMetaStorage;
        private readonly TimeSpan tasksTtl = TimeSpan.FromSeconds(5);
    }
}