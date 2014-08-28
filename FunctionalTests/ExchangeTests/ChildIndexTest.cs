using NUnit.Framework;

using RemoteQueue.Handling;

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

        private IRemoteTaskQueue taskQueue;
    }
}