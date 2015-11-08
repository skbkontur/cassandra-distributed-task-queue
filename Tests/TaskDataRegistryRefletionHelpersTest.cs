using System;

using NUnit.Framework;

using RemoteQueue.Configuration;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.Tests
{
    [TestFixture]
    public class TaskDataRegistryRefletionHelpersTest
    {
        [Test]
        public void GetTaskName()
        {
            Assert.Throws<InvalidProgramStateException>(() => typeof(TaskData0).GetTaskName());
            Assert.That(typeof(TaskData1).GetTaskName(), Is.EqualTo("TaskName1"));
            Assert.That(typeof(TaskData2).GetTaskName(), Is.EqualTo("TaskName2"));
            Assert.That(typeof(TaskData3).GetTaskName(), Is.EqualTo("TaskName3"));
        }

        [Test]
        public void TryGetTaskTopic()
        {
            Assert.That(typeof(TaskData0).TryGetTaskTopic(), Is.Null);
            Assert.That(typeof(TaskData1).TryGetTaskTopic(), Is.EqualTo("Topic1"));
            Assert.That(typeof(TaskData2).TryGetTaskTopic(), Is.EqualTo("Topic2"));
            Assert.Throws<InvalidOperationException>(() => typeof(TaskData3).TryGetTaskTopic());
        }

        private abstract class BaseTaskData : ITaskData
        {
        }

        [TaskName("BaseName")]
        private abstract class BaseTaskDataWithName : ITaskData
        {
        }

        [TaskTopic("BaseTopic")]
        private abstract class BaseTaskDataWithTopic : ITaskData
        {
        }

        private class TaskData0 : BaseTaskData
        {
        }

        [TaskName("TaskName1")]
        [TaskTopic("Topic1")]
        private class TaskData1 : BaseTaskData
        {
        }

        [TaskName("TaskName2")]
        [TaskTopic("Topic2")]
        private class TaskData2 : BaseTaskDataWithName
        {
        }

        [TaskName("TaskName3")]
        [TaskTopic("Topic3")]
        private class TaskData3 : BaseTaskDataWithTopic
        {
        }
    }
}