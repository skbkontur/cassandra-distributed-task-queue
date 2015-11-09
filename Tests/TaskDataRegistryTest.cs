using System;

using NUnit.Framework;

using RemoteQueue.Configuration;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.Tests
{
    [TestFixture]
    public class TaskDataRegistryTest
    {
        [Test]
        public void GetAllTaskNames()
        {
            Assert.That(taskDataRegistry.GetAllTaskNames(), Is.EquivalentTo(new[] {"TaskData1", "TaskData2", "TaskData3", "TaskData1WithTopic1", "TaskData2WithTopic1", "TaskData3WithTopic2", "TaskData4WithTopic3"}));
        }

        [Test]
        public void GetTaskName()
        {
            Assert.That(taskDataRegistry.GetTaskName(typeof(TaskData1)), Is.EqualTo("TaskData1"));
            Assert.That(taskDataRegistry.GetTaskName(typeof(TaskData2)), Is.EqualTo("TaskData2"));
            Assert.That(taskDataRegistry.GetTaskName(typeof(TaskData3)), Is.EqualTo("TaskData3"));
            Assert.That(taskDataRegistry.GetTaskName(typeof(TaskData1WithTopic1)), Is.EqualTo("TaskData1WithTopic1"));
            Assert.That(taskDataRegistry.GetTaskName(typeof(TaskData2WithTopic1)), Is.EqualTo("TaskData2WithTopic1"));
            Assert.That(taskDataRegistry.GetTaskName(typeof(TaskData3WithTopic2)), Is.EqualTo("TaskData3WithTopic2"));
            Assert.That(taskDataRegistry.GetTaskName(typeof(TaskData4WithTopic3)), Is.EqualTo("TaskData4WithTopic3"));
            Assert.Throws<InvalidProgramStateException>(() => taskDataRegistry.GetTaskName(typeof(string)));
        }

        [Test]
        public void GetTaskType()
        {
            Assert.That(taskDataRegistry.GetTaskType("TaskData1"), Is.EqualTo(typeof(TaskData1)));
            Assert.That(taskDataRegistry.GetTaskType("TaskData2"), Is.EqualTo(typeof(TaskData2)));
            Assert.That(taskDataRegistry.GetTaskType("TaskData3"), Is.EqualTo(typeof(TaskData3)));
            Assert.That(taskDataRegistry.GetTaskType("TaskData1WithTopic1"), Is.EqualTo(typeof(TaskData1WithTopic1)));
            Assert.That(taskDataRegistry.GetTaskType("TaskData2WithTopic1"), Is.EqualTo(typeof(TaskData2WithTopic1)));
            Assert.That(taskDataRegistry.GetTaskType("TaskData3WithTopic2"), Is.EqualTo(typeof(TaskData3WithTopic2)));
            Assert.That(taskDataRegistry.GetTaskType("TaskData4WithTopic3"), Is.EqualTo(typeof(TaskData4WithTopic3)));
            Assert.Throws<InvalidProgramStateException>(() => taskDataRegistry.GetTaskType("UnregisteredTask"));
        }

        [Test]
        public void TryGetTaskType()
        {
            Assert.That(TryGetTaskType("TaskData1"), Is.EqualTo(typeof(TaskData1)));
            Assert.That(TryGetTaskType("TaskData2"), Is.EqualTo(typeof(TaskData2)));
            Assert.That(TryGetTaskType("TaskData3"), Is.EqualTo(typeof(TaskData3)));
            Assert.That(TryGetTaskType("TaskData1WithTopic1"), Is.EqualTo(typeof(TaskData1WithTopic1)));
            Assert.That(TryGetTaskType("TaskData2WithTopic1"), Is.EqualTo(typeof(TaskData2WithTopic1)));
            Assert.That(TryGetTaskType("TaskData3WithTopic2"), Is.EqualTo(typeof(TaskData3WithTopic2)));
            Assert.That(TryGetTaskType("TaskData4WithTopic3"), Is.EqualTo(typeof(TaskData4WithTopic3)));
            Assert.That(TryGetTaskType("UnregisteredTask"), Is.Null);
        }

        private Type TryGetTaskType(string taskName)
        {
            Type taskType;
            taskDataRegistry.TryGetTaskType(taskName, out taskType);
            return taskType;
        }

        [Test]
        public void GetAllTaskTopics()
        {
            Assert.That(taskDataRegistry.GetAllTaskTopics(), Is.EquivalentTo(new[] {"0", "1", "Topic1", "Topic2", "Topic3"}));
        }

        [Test]
        public void GetTaskTopic()
        {
            Assert.That(taskDataRegistry.GetTaskTopic("TaskData1"), Is.EqualTo("0"));
            Assert.That(taskDataRegistry.GetTaskTopic("TaskData2"), Is.EqualTo("1"));
            Assert.That(taskDataRegistry.GetTaskTopic("TaskData3"), Is.EqualTo("0"));
            Assert.That(taskDataRegistry.GetTaskTopic("TaskData1WithTopic1"), Is.EqualTo("Topic1"));
            Assert.That(taskDataRegistry.GetTaskTopic("TaskData2WithTopic1"), Is.EqualTo("Topic1"));
            Assert.That(taskDataRegistry.GetTaskTopic("TaskData3WithTopic2"), Is.EqualTo("Topic2"));
            Assert.That(taskDataRegistry.GetTaskTopic("TaskData4WithTopic3"), Is.EqualTo("Topic3"));
            Assert.Throws<InvalidProgramStateException>(() => taskDataRegistry.GetTaskTopic("UnregisteredTask"));
        }

        [TaskTopic("Topic3")]
        private interface ITaskDataWithTopic : ITaskData
        {
        }

        private readonly TestTaskDataRegistry taskDataRegistry = new TestTaskDataRegistry();

        private class TestTaskDataRegistry : TaskDataRegistryBase
        {
            public TestTaskDataRegistry()
            {
                Register<TaskData1>();
                Register<TaskData2>();
                Register<TaskData3>();
                Register<TaskData1WithTopic1>();
                Register<TaskData2WithTopic1>();
                Register<TaskData3WithTopic2>();
                Register<TaskData4WithTopic3>();
            }
        }

        [TaskName("TaskData1")]
        private class TaskData1 : ITaskData
        {
        }

        [TaskName("TaskData2")]
        private class TaskData2 : ITaskData
        {
        }

        [TaskName("TaskData3")]
        private class TaskData3 : ITaskData
        {
        }

        [TaskName("TaskData1WithTopic1"), TaskTopic("Topic1")]
        private class TaskData1WithTopic1 : ITaskData
        {
        }

        [TaskName("TaskData2WithTopic1"), TaskTopic("Topic1")]
        private class TaskData2WithTopic1 : ITaskData
        {
        }

        [TaskName("TaskData3WithTopic2"), TaskTopic("Topic2")]
        private class TaskData3WithTopic2 : ITaskData
        {
        }

        [TaskName("TaskData4WithTopic3")]
        private class TaskData4WithTopic3 : ITaskDataWithTopic
        {
        }
    }
}