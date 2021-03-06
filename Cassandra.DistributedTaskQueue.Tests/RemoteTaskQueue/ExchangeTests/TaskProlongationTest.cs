﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using GroboContainer.NUnitExtensions;

using NUnit.Framework;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.BlobStorages;
using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common;
using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.TaskDatas;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.DistributedTaskQueue.Tests.RemoteTaskQueue.RepositoriesTests;
using SkbKontur.Cassandra.TimeBasedUuid;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests.RemoteTaskQueue.ExchangeTests
{
    public class TaskProlongationTest : TasksWithCounterTestBase
    {
        [GroboSetUp]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public void SetUp()
        {
            var smallTtlRemoteTaskQueueSettings = new SmallTtlRtqSettings(new TestRtqSettings(), smallTaskTtl);
            smallTtlRemoteTaskQueue = GroboTestContext.Current.Container.Create<IRtqSettings, SkbKontur.Cassandra.DistributedTaskQueue.Handling.RemoteTaskQueue>(smallTtlRemoteTaskQueueSettings);
        }

        [Test]
        public void Prolongation_Happened()
        {
            SetTaskTtlOnConsumers(TimeSpan.FromHours(1));

            var rerunAfter = TimeSpan.FromMilliseconds(100);

            var estimatedNumberOfRuns = 2 * (int)(smallTaskTtl.Ticks / rerunAfter.Ticks);
            Log.For(this).Info("Estimated number of runs: {EstimatedNumberOfRuns}", new {EstimatedNumberOfRuns = estimatedNumberOfRuns});
            var parentTaskId = Guid.NewGuid().ToString();
            var taskId = AddTask(estimatedNumberOfRuns, rerunAfter, new[] {estimatedNumberOfRuns, estimatedNumberOfRuns - 1, estimatedNumberOfRuns - 2}, parentTaskId);
            WaitForTerminalState(new[] {taskId}, TaskState.Finished, "FakeMixedPeriodicAndFailTaskData", TimeSpan.FromMinutes(1), TimeSpan.FromMilliseconds(100));

            Thread.Sleep(10000);
            var taskInfo = smallTtlRemoteTaskQueue.GetTaskInfo<FakeMixedPeriodicAndFailTaskData>(taskId);
            Assert.That(taskInfo.ExceptionInfos.Length, Is.EqualTo(3));
            Assert.That(smallTtlRemoteTaskQueue.GetChildrenTaskIds(parentTaskId), Is.EquivalentTo(new[] {taskId}));
        }

        [Test]
        public void Prolongation_Happened_MoreThanOnce()
        {
            SetTaskTtlOnConsumers(smallTaskTtl);

            var rerunAfter = TimeSpan.FromSeconds(1);

            var estimatedNumberOfRuns = (int)(smallTaskTtl.Ticks * 10 / rerunAfter.Ticks);
            Log.For(this).Info("Estimated number of runs: {EstimatedNumberOfRuns}", new {EstimatedNumberOfRuns = estimatedNumberOfRuns});
            var taskId = AddTask(estimatedNumberOfRuns, rerunAfter, null, null);
            WaitForTerminalState(new[] {taskId}, TaskState.Finished, "FakeMixedPeriodicAndFailTaskData", TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(1));
            var now = Timestamp.Now;
            var taskInfo = smallTtlRemoteTaskQueue.GetTaskInfo<FakeMixedPeriodicAndFailTaskData>(taskId);
            Assert.That(taskInfo.Context.ExpirationTimestampTicks, Is.InRange((now - smallTaskTtl).Ticks, (now + smallTaskTtl).Ticks));
        }

        [Test]
        public void Prolongation_NotHappened()
        {
            SetTaskTtlOnConsumers(TimeSpan.FromHours(1));

            var parentTaskId = Guid.NewGuid().ToString();
            var taskId = AddTask(2, TimeSpan.FromMilliseconds(100), new[] {2}, parentTaskId);
            WaitForTerminalState(new[] {taskId}, TaskState.Finished, "FakeMixedPeriodicAndFailTaskData", TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(100));
            var meta = smallTtlRemoteTaskQueue.GetTaskInfo<FakeMixedPeriodicAndFailTaskData>(taskId).Context;

            Assert.That(() => smallTtlRemoteTaskQueue.GetTaskInfos<FakeMixedPeriodicAndFailTaskData>(new[] {taskId}), Is.Empty.After(10000, 100));
            Assert.That(() => taskMetaStorage.Read(taskId), Is.Null.After(10000, 100));
            Assert.That(() => taskDataStorage.Read(meta), Is.Null.After(10000, 100));
            Assert.That(() => taskExceptionInfoStorage.Read(new[] {meta})[meta.Id], Is.Empty.After(10000, 100));
            Assert.That(() => smallTtlRemoteTaskQueue.GetChildrenTaskIds(parentTaskId), Is.Empty.After(10000, 100));
        }

        [Test]
        public void Prolongation_FarRerunProtection()
        {
            SetTaskTtlOnConsumers(smallTaskTtl);

            var bigRerunPeriod = TimeSpan.FromDays(1440);
            var taskId = AddTask(2, bigRerunPeriod, new[] {2}, null);
            WaitForState(new[] {taskId}, TaskState.WaitingForRerunAfterError, TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(100));

            Thread.Sleep(10000);
            Assert.DoesNotThrow(() => smallTtlRemoteTaskQueue.GetTaskInfo<FakeMixedPeriodicAndFailTaskData>(taskId));
        }

        private string AddTask(int attempts, TimeSpan rerunAfter, int[] failCounterValues, string parentTaskId)
        {
            var task = smallTtlRemoteTaskQueue.CreateTask(new FakeMixedPeriodicAndFailTaskData(rerunAfter, failCounterValues),
                                                          new CreateTaskOptions
                                                              {
                                                                  ParentTaskId = parentTaskId
                                                              });
            testCounterRepository.SetValueForCounter(task.Id, attempts);
            task.Queue();
            return task.Id;
        }

        private static void SetTaskTtlOnConsumers(TimeSpan ttl)
        {
            GroboTestContext.Current.Container.Get<ExchangeServiceClient>().ChangeTaskTtl(ttl);
        }

        private SkbKontur.Cassandra.DistributedTaskQueue.Handling.RemoteTaskQueue smallTtlRemoteTaskQueue;

        [Injected]
        private readonly ITaskMetaStorage taskMetaStorage;

        [Injected]
        private readonly ITaskDataStorage taskDataStorage;

        [Injected]
        private readonly ITaskExceptionInfoStorage taskExceptionInfoStorage;

        private readonly TimeSpan smallTaskTtl = TimeSpan.FromSeconds(5);
    }
}