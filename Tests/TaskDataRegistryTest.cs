using System.Collections.Generic;

using NUnit.Framework;

using RemoteQueue.Configuration;
using RemoteQueue.Handling;

namespace RemoteQueue.Tests
{
    [TestFixture]
    public class TaskDataRegistryTest
    {
        [Test]
        public void GetAllTaskTopics()
        {
            Assert.That(taskDataRegistry.GetAllTaskTopics().Length, Is.LessThanOrEqualTo(2));
            Assert.That(taskDataRegistry.GetAllTaskTopics(), Is.EquivalentTo(new[] {"0", "1"}));
        }

        [Test]
        public void GetTaskTopic()
        {
            Assert.That(taskDataRegistry.GetTaskTopic("TaskData1"), Is.EqualTo("0"));
            Assert.That(taskDataRegistry.GetTaskTopic("TaskData2"), Is.EqualTo("1"));
            Assert.That(taskDataRegistry.GetTaskTopic("TaskData3"), Is.EqualTo("0"));
            Assert.Throws<KeyNotFoundException>(() => taskDataRegistry.GetTaskTopic("UnregisteredTask"));
        }

        private readonly TestTaskDataRegistry taskDataRegistry = new TestTaskDataRegistry();

        private class TestTaskDataRegistry : TaskDataRegistryBase
        {
            public TestTaskDataRegistry()
            {
                Register<TaskData1>("TaskData1");
                Register<TaskData2>("TaskData2");
                Register<TaskData3>("TaskData3");
            }
        }

        private class TaskData1 : ITaskData
        {
        }

        private class TaskData2 : ITaskData
        {
        }

        private class TaskData3 : ITaskData
        {
        }
    }
}