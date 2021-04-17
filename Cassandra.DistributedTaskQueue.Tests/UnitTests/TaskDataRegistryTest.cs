using System;

using NUnit.Framework;

using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests.UnitTests
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
            Assert.Throws<InvalidOperationException>(() => taskDataRegistry.GetTaskName(typeof(string)));
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
            Assert.Throws<InvalidOperationException>(() => taskDataRegistry.GetTaskType("UnregisteredTask"));
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
            Assert.Throws<InvalidOperationException>(() => taskDataRegistry.GetTaskTopic("UnregisteredTask"));
        }

        [RtqTaskTopic("Topic3")]
        private interface ITaskDataWithTopic : IRtqTaskData
        {
        }

        private readonly TestTaskDataRegistry taskDataRegistry = new TestTaskDataRegistry();

        private class TestTaskDataRegistry : RtqTaskDataRegistryBase
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

        [RtqTaskName("TaskData1")]
        private class TaskData1 : IRtqTaskData
        {
        }

        [RtqTaskName("TaskData2")]
        private class TaskData2 : IRtqTaskData
        {
        }

        [RtqTaskName("TaskData3")]
        private class TaskData3 : IRtqTaskData
        {
        }

        [RtqTaskName("TaskData1WithTopic1"), RtqTaskTopic("Topic1")]
        private class TaskData1WithTopic1 : IRtqTaskData
        {
        }

        [RtqTaskName("TaskData2WithTopic1"), RtqTaskTopic("Topic1")]
        private class TaskData2WithTopic1 : IRtqTaskData
        {
        }

        [RtqTaskName("TaskData3WithTopic2"), RtqTaskTopic("Topic2")]
        private class TaskData3WithTopic2 : IRtqTaskData
        {
        }

        [RtqTaskName("TaskData4WithTopic3")]
        private class TaskData4WithTopic3 : ITaskDataWithTopic
        {
        }
    }
}