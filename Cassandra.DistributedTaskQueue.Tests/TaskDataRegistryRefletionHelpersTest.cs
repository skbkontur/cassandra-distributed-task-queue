using System;

using NUnit.Framework;

using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests
{
    [TestFixture]
    public class TaskDataRegistryRefletionHelpersTest
    {
        [Test]
        public void GetTaskName()
        {
            Assert.Throws<InvalidOperationException>(() => typeof(TaskData0).GetTaskName());
            Assert.That(typeof(TaskData1).GetTaskName(), Is.EqualTo("TaskName1"));
            Assert.That(typeof(TaskData2).GetTaskName(), Is.EqualTo("TaskName2"));
            Assert.That(typeof(TaskData3).GetTaskName(), Is.EqualTo("TaskName3"));
        }

        [Test]
        public void TryGetTaskTopic()
        {
            Assert.That(typeof(TaskData0).TryGetTaskTopic(false), Is.Null);
            Assert.Throws<InvalidOperationException>(() => typeof(TaskData0).TryGetTaskTopic(true));
            Assert.That(typeof(TaskData1).TryGetTaskTopic(false), Is.EqualTo("Topic1"));
            Assert.That(typeof(TaskData2).TryGetTaskTopic(false), Is.EqualTo("Topic2"));
            Assert.Throws<InvalidOperationException>(() => typeof(TaskData3).TryGetTaskTopic(false));
        }

        private abstract class BaseTaskData : IRtqTaskData
        {
        }

        [RtqTaskName("BaseName")]
        private abstract class BaseTaskDataWithName : IRtqTaskData
        {
        }

        [RtqTaskTopic("BaseTopic")]
        private abstract class BaseTaskDataWithTopic : IRtqTaskData
        {
        }

        private class TaskData0 : BaseTaskData
        {
        }

        [RtqTaskName("TaskName1")]
        [RtqTaskTopic("Topic1")]
        private class TaskData1 : BaseTaskData
        {
        }

        [RtqTaskName("TaskName2")]
        [RtqTaskTopic("Topic2")]
        private class TaskData2 : BaseTaskDataWithName
        {
        }

        [RtqTaskName("TaskName3")]
        [RtqTaskTopic("Topic3")]
        private class TaskData3 : BaseTaskDataWithTopic
        {
        }
    }
}