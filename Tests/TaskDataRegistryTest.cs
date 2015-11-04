using System.Collections.Generic;

using NUnit.Framework;

using SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue;

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
            Assert.That(taskDataRegistry.GetTaskTopic("SimpleTaskData"), Is.EqualTo("1"));
            Assert.Throws<KeyNotFoundException>(() => taskDataRegistry.GetTaskTopic("UnregisteredTask"));
        }

        private readonly TaskDataRegistry taskDataRegistry = new TaskDataRegistry();
    }
}