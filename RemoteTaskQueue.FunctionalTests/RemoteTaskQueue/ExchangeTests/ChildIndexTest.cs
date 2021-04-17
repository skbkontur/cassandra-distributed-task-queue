using System;

using GroboContainer.NUnitExtensions;

using NUnit.Framework;

using RemoteTaskQueue.FunctionalTests.Common;
using RemoteTaskQueue.FunctionalTests.Common.TaskDatas;
using RemoteTaskQueue.FunctionalTests.RemoteTaskQueue.RepositoriesTests;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace RemoteTaskQueue.FunctionalTests.RemoteTaskQueue.ExchangeTests
{
    public class ChildIndexTest : ExchangeTestBase
    {
        [Test]
        public void NoChildrenTasksTest()
        {
            var taskId = remoteTaskQueue.CreateTask(new SimpleTaskData()).Queue();
            CollectionAssert.IsEmpty(remoteTaskQueue.GetChildrenTaskIds(taskId));
        }

        [Test]
        public void MultipleChildrenTest()
        {
            var taskId = remoteTaskQueue.CreateTask(new SimpleTaskData()).Queue();
            var childTaskId1 = remoteTaskQueue.CreateTask(new SimpleTaskData(), new CreateTaskOptions {ParentTaskId = taskId}).Queue();
            var childTaskId2 = remoteTaskQueue.CreateTask(new SimpleTaskData(), new CreateTaskOptions {ParentTaskId = taskId}).Queue();
            var childTaskId3 = remoteTaskQueue.CreateTask(new SimpleTaskData(), new CreateTaskOptions {ParentTaskId = taskId}).Queue();
            CollectionAssert.AreEquivalent(new[] {childTaskId1, childTaskId2, childTaskId3}, remoteTaskQueue.GetChildrenTaskIds(taskId));
        }

        [Test]
        public void ChainTest()
        {
            var taskId = remoteTaskQueue.CreateTask(new SimpleTaskData()).Queue();
            var childTaskId = remoteTaskQueue.CreateTask(new SimpleTaskData(), new CreateTaskOptions {ParentTaskId = taskId}).Queue();
            var grandChildTaskId = remoteTaskQueue.CreateTask(new SimpleTaskData(), new CreateTaskOptions {ParentTaskId = childTaskId}).Queue();
            CollectionAssert.AreEqual(new[] {childTaskId}, remoteTaskQueue.GetChildrenTaskIds(taskId));
            CollectionAssert.AreEqual(new[] {grandChildTaskId}, remoteTaskQueue.GetChildrenTaskIds(childTaskId));
        }

        [Test]
        [Repeat(5)]
        public void TtlTest()
        {
            GroboTestContext.Current.Container.Get<ExchangeServiceClient>().Stop();

            var smallTtlRemoteTaskQueueSettings = new SmallTtlRtqSettings(new TestRtqSettings(), TimeSpan.FromSeconds(5));
            var smallTtlRemoteTaskQueue = GroboTestContext.Current.Container.Create<IRtqSettings, SkbKontur.Cassandra.DistributedTaskQueue.Handling.RemoteTaskQueue>(smallTtlRemoteTaskQueueSettings);

            var taskId = smallTtlRemoteTaskQueue.CreateTask(new SimpleTaskData()).Queue();
            var childTaskId1 = smallTtlRemoteTaskQueue.CreateTask(new SimpleTaskData(), new CreateTaskOptions {ParentTaskId = taskId}).Queue();
            var childTaskId2 = smallTtlRemoteTaskQueue.CreateTask(new SimpleTaskData(), new CreateTaskOptions {ParentTaskId = taskId}).Queue();
            var childTaskId3 = smallTtlRemoteTaskQueue.CreateTask(new SimpleTaskData(), new CreateTaskOptions {ParentTaskId = taskId}).Queue();
            CollectionAssert.AreEquivalent(new[] {childTaskId1, childTaskId2, childTaskId3}, smallTtlRemoteTaskQueue.GetChildrenTaskIds(taskId));
            Assert.That(() => smallTtlRemoteTaskQueue.GetChildrenTaskIds(taskId), Is.Empty.After(10000, 100));
        }
    }
}