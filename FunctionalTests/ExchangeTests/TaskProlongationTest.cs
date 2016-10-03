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
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.RemoteTaskQueue.Common;
using SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue;
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

        public override void TearDown()
        {
            base.TearDown();
            SetTaskTtlOnConsumers(RemoteQueueTestsCassandraSettings.StandardTestTaskTtl);
        }

        [Test]
        public void Prolongation_Happened()
        {
            SetTaskTtlOnConsumers(TimeSpan.FromHours(1));

            var rerunAfter = TimeSpan.FromMilliseconds(100);

            var estimatedNumberOfRuns = 2 * (int)(tasksTtl.Ticks / rerunAfter.Ticks);
            Console.WriteLine("Estimated number of runs: {0}", estimatedNumberOfRuns);
            var parentTaskId = Guid.NewGuid().ToString();
            var taskId = AddTask(estimatedNumberOfRuns, rerunAfter, new[] { estimatedNumberOfRuns, estimatedNumberOfRuns - 1, estimatedNumberOfRuns - 2 }, parentTaskId);
            WaitForTerminalState(new[] { taskId }, TaskState.Finished, "FakeMixedPeriodicAndFailTaskData", TimeSpan.FromMinutes(1), TimeSpan.FromMilliseconds(100));
            
            Thread.Sleep(10000);
            var taskInfo = taskQueue.GetTaskInfo<FakeMixedPeriodicAndFailTaskData>(taskId);
            Assert.That(taskInfo.ExceptionInfos.Length, Is.EqualTo(3));
            Assert.That(taskQueue.GetChildrenTaskIds(parentTaskId), Is.EquivalentTo(new []{taskId}));
        }

        [Test]
        public void Prolongation_Happened_MoreThanOnce()
        {
            SetTaskTtlOnConsumers(tasksTtl);

            var rerunAfter = TimeSpan.FromSeconds(1);

            var estimatedNumberOfRuns = (int) (tasksTtl.Ticks * 10 / rerunAfter.Ticks);
            Console.WriteLine("Estimated number of runs: {0}", estimatedNumberOfRuns);
            var taskId = AddTask(estimatedNumberOfRuns, rerunAfter, null, null);
            WaitForTerminalState(new[] {taskId}, TaskState.Finished, "FakeMixedPeriodicAndFailTaskData", TimeSpan.FromMinutes(2), TimeSpan.FromMilliseconds(100));
            var now = Timestamp.Now;
            var taskInfo = taskQueue.GetTaskInfo<FakeMixedPeriodicAndFailTaskData>(taskId);
            Assert.That(taskInfo.Context.ExpirationTimestampTicks, Is.InRange((now - tasksTtl).Ticks, (now + tasksTtl).Ticks));
        }

        [Test]
        public void Prolongation_NotHappened()
        {
            SetTaskTtlOnConsumers(TimeSpan.FromHours(1));

            var parentTaskId = Guid.NewGuid().ToString();
            var taskId = AddTask(2, TimeSpan.FromMilliseconds(100), new[]{2}, parentTaskId);
            WaitForTerminalState(new[] {taskId}, TaskState.Finished, "FakeMixedPeriodicAndFailTaskData", TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(100));
            var meta = taskQueue.GetTaskInfo<FakeMixedPeriodicAndFailTaskData>(taskId).Context;

            Assert.That(() => taskQueue.GetTaskInfos<FakeMixedPeriodicAndFailTaskData>(new[] {taskId}), Is.Empty.After(10000, 100));
            Assert.That(() => taskMetaStorage.Read(taskId), Is.Null.After(10000, 100));
            Assert.That(() => taskDataStorage.Read(meta), Is.Null.After(10000, 100));
            Assert.That(() => taskExceptionInfoStorage.Read(new[] {meta})[meta.Id], Is.Empty.After(10000, 100));
            Assert.That(() => taskQueue.GetChildrenTaskIds(parentTaskId), Is.Empty.After(10000, 100));
        }

        [Test]
        public void Prolongation_FarRerunProtection()
        {
            SetTaskTtlOnConsumers(tasksTtl);

            var bigRerunPeriod = TimeSpan.FromDays(1440);
            var taskId = AddTask(2, bigRerunPeriod, new[] { 2 }, null);
            WaitForState(new[] {taskId}, TaskState.WaitingForRerunAfterError, TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(100));
            Assert.That(taskQueue.GetTaskInfo<FakeMixedPeriodicAndFailTaskData>(taskId).Context.TtlTicks, Is.GreaterThanOrEqualTo(bigRerunPeriod.Ticks));

            Thread.Sleep(10000);
            Assert.DoesNotThrow(() => taskQueue.GetTaskInfo<FakeMixedPeriodicAndFailTaskData>(taskId));
        }

        private string AddTask(int attempts, TimeSpan rerunAfter, int[] failCounterValues, string parentTaskId)
        {
            var task = taskQueue.CreateTask(new FakeMixedPeriodicAndFailTaskData(rerunAfter, failCounterValues), new CreateTaskOptions
                {
                    ParentTaskId = parentTaskId
                });
            testCounterRepository.SetValueForCounter(task.Id, attempts);
            task.Queue();
            return task.Id;
        }

        private void SetTaskTtlOnConsumers(TimeSpan ttl)
        {
            Container.Get<IExchangeServiceClient>().ChangeTaskTtl(ttl);
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