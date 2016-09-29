using System;

using FunctionalTests.RepositoriesTests;

using GroBuf;

using NUnit.Framework;

using RemoteQueue.Configuration;
using RemoteQueue.Handling;
using RemoteQueue.Profiling;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas;

namespace FunctionalTests.ExchangeTests
{
    public class ChildIndexTest : FunctionalTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            taskQueue = Container.Get<IRemoteTaskQueue>();
        }

        [Test]
        public void NoChildrenTasksTest()
        {
            var taskId = taskQueue.CreateTask(new SimpleTaskData()).Queue();
            CollectionAssert.IsEmpty(taskQueue.GetChildrenTaskIds(taskId));
        }

        [Test]
        public void MultipleChildrenTest()
        {
            var taskId = taskQueue.CreateTask(new SimpleTaskData()).Queue();
            var childTaskId1 = taskQueue.CreateTask(new SimpleTaskData(), new CreateTaskOptions {ParentTaskId = taskId}).Queue();
            var childTaskId2 = taskQueue.CreateTask(new SimpleTaskData(), new CreateTaskOptions {ParentTaskId = taskId}).Queue();
            var childTaskId3 = taskQueue.CreateTask(new SimpleTaskData(), new CreateTaskOptions {ParentTaskId = taskId}).Queue();
            CollectionAssert.AreEquivalent(new[] {childTaskId1, childTaskId2, childTaskId3}, taskQueue.GetChildrenTaskIds(taskId));
        }

        [Test]
        public void ChainTest()
        {
            var taskId = taskQueue.CreateTask(new SimpleTaskData()).Queue();
            var childTaskId = taskQueue.CreateTask(new SimpleTaskData(), new CreateTaskOptions {ParentTaskId = taskId}).Queue();
            var grandChildTaskId = taskQueue.CreateTask(new SimpleTaskData(), new CreateTaskOptions {ParentTaskId = childTaskId}).Queue();
            CollectionAssert.AreEqual(new[] {childTaskId}, taskQueue.GetChildrenTaskIds(taskId));
            CollectionAssert.AreEqual(new[] {grandChildTaskId}, taskQueue.GetChildrenTaskIds(childTaskId));
        }

        [Test]
        public void TtlTest()
        {
            taskQueue = new RemoteTaskQueue(
                Container.Get<ISerializer>(),
                Container.Get<ICassandraCluster>(),
                new SmallTtlRemoteTaskQueueSettings(Container.Get<IRemoteTaskQueueSettings>(), TimeSpan.FromSeconds(5)),
                Container.Get<ITaskDataRegistry>(),
                Container.Get<IRemoteTaskQueueProfiler>());

            var taskId = taskQueue.CreateTask(new SimpleTaskData()).Queue();
            var childTaskId1 = taskQueue.CreateTask(new SimpleTaskData(), new CreateTaskOptions {ParentTaskId = taskId}).Queue();
            var childTaskId2 = taskQueue.CreateTask(new SimpleTaskData(), new CreateTaskOptions {ParentTaskId = taskId}).Queue();
            var childTaskId3 = taskQueue.CreateTask(new SimpleTaskData(), new CreateTaskOptions {ParentTaskId = taskId}).Queue();
            CollectionAssert.AreEquivalent(new[] {childTaskId1, childTaskId2, childTaskId3}, taskQueue.GetChildrenTaskIds(taskId));
            Assert.That(() => taskQueue.GetChildrenTaskIds(taskId), Is.Empty.After(10000, 100));
        }

        private IRemoteTaskQueue taskQueue;
    }
}