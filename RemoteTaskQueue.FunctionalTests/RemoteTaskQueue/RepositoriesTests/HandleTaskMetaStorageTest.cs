using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using GroboContainer.Infection;
using GroboContainer.NUnitExtensions;
using GroboContainer.NUnitExtensions.Impl.TestContext;

using NUnit.Framework;

using RemoteTaskQueue.FunctionalTests.Common;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes;
using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.TimeBasedUuid;

using SKBKontur.Catalogue.TestCore.NUnit.Extensions;

namespace RemoteTaskQueue.FunctionalTests.RemoteTaskQueue.RepositoriesTests
{
    [GroboTestFixture]
    [WithDefaultSerializer, WithTestRtqSettings, WithCassandra(TestRtqSettings.QueueKeyspaceName), AndResetCassandraState]
    public class HandleTaskMetaStorageTest
    {
        [GroboTestFixtureSetUp]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public void TestFixtureSetUp(IEditableGroboTestContext suiteContext)
        {
            taskDataRegistry = new DummyTaskDataRegistry();
            sut = suiteContext.Container.Create<IRtqTaskDataRegistry, HandleTasksMetaStorage>(taskDataRegistry);
        }

        private static string NewTaskId()
        {
            return TimeGuid.NowGuid().ToGuid().ToString();
        }

        private TaskIndexShardKey TaskIndexShardKey(string taskName, TaskState taskState)
        {
            return new TaskIndexShardKey(taskDataRegistry.GetTaskTopic(taskName), taskState);
        }

        [Test]
        public void StressTest()
        {
            var nowTicks = Timestamp.Now.Ticks;
            var meta = new TaskMetaInformation("TaskName", NewTaskId()) {State = TaskState.New, MinimalStartTicks = nowTicks + 1};
            for (var i = 0; i <= 1000; i++)
            {
                var oldTaskIndexRecord = sut.FormatIndexRecord(meta);
                meta.MinimalStartTicks++;
                sut.AddMeta(meta, oldTaskIndexRecord);
            }
            Assert.AreEqual(1, sut.GetIndexRecords(nowTicks + 1002, new[] {TaskIndexShardKey("TaskName", TaskState.New)}).Length);
        }

        [Test]
        public void StressTest2()
        {
            var nowTicks = Timestamp.Now.Ticks;
            var metas = new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.Select(x =>
                {
                    var taskMeta = new TaskMetaInformation("TaskName", NewTaskId()) {State = TaskState.New, MinimalStartTicks = nowTicks + x};
                    return taskMeta;
                }).ToArray();
            for (var i = 0; i <= 100; i++)
            {
                foreach (var t in metas)
                {
                    var oldTaskIndexRecord = sut.FormatIndexRecord(t);
                    t.MinimalStartTicks++;
                    t.State = i % 2 == 0 ? TaskState.Finished : TaskState.New;
                    sut.AddMeta(t, oldTaskIndexRecord);
                }
            }
            Assert.AreEqual(10, sut.GetIndexRecords(nowTicks + 1012, new[] {TaskIndexShardKey("TaskName", TaskState.Finished)}).Length);
        }

        [Test]
        public void SimpleTest()
        {
            var ticks = Timestamp.Now.Ticks;
            var id = NewTaskId();
            var taskMeta = new TaskMetaInformation("TaskName", id) {State = TaskState.New, MinimalStartTicks = ticks};
            sut.AddMeta(taskMeta, oldTaskIndexRecord : null);
            var tasks = sut.GetIndexRecords(ticks + 1, new[] {TaskIndexShardKey("TaskName", TaskState.New)});
            Assert.AreEqual(1, tasks.Length);
            Assert.AreEqual(id, tasks[0].TaskId);
            tasks = sut.GetIndexRecords(ticks, new[] {TaskIndexShardKey("TaskName", TaskState.New)});
            Assert.AreEqual(1, tasks.Length);
            Assert.AreEqual(id, tasks[0].TaskId);
            tasks = sut.GetIndexRecords(ticks - 1, new[] {TaskIndexShardKey("TaskName", TaskState.New)});
            Assert.AreEqual(0, tasks.Length);
        }

        [Test]
        public void DifferentStatesTest()
        {
            var ticks = Timestamp.Now.Ticks;
            var taskMeta1 = new TaskMetaInformation("TaskName", NewTaskId()) {State = TaskState.InProcess, MinimalStartTicks = ticks};
            sut.AddMeta(taskMeta1, oldTaskIndexRecord : null);
            var taskMeta2 = new TaskMetaInformation("TaskName", NewTaskId()) {State = TaskState.Finished, MinimalStartTicks = ticks};
            sut.AddMeta(taskMeta2, oldTaskIndexRecord : null);
            var tasks = sut.GetIndexRecords(ticks + 1, new[] {TaskIndexShardKey("TaskName", TaskState.InProcess)});
            Assert.AreEqual(1, tasks.Length);
        }

        [Test]
        public void DifferentNamesTest()
        {
            var ticks = Timestamp.Now.Ticks;
            var taskMeta1 = new TaskMetaInformation("TaskName1", NewTaskId()) {State = TaskState.New, MinimalStartTicks = ticks};
            sut.AddMeta(taskMeta1, oldTaskIndexRecord : null);
            var taskMeta2 = new TaskMetaInformation("TaskName2", NewTaskId()) {State = TaskState.New, MinimalStartTicks = ticks};
            sut.AddMeta(taskMeta2, oldTaskIndexRecord : null);
            var tasks = sut.GetIndexRecords(ticks + 1, new[] {TaskIndexShardKey("TaskName1", TaskState.New)});
            Assert.AreEqual(1, tasks.Length);
        }

        [Test]
        public void ManyTasksTest()
        {
            var ticks = Timestamp.Now.Ticks;
            var id1 = NewTaskId();
            var id2 = NewTaskId();
            var id3 = NewTaskId();
            var id4 = NewTaskId();
            var taskMeta1 = new TaskMetaInformation("TaskName", id1) {State = TaskState.New, MinimalStartTicks = ticks + 10};
            sut.AddMeta(taskMeta1, oldTaskIndexRecord : null);
            var taskMeta2 = new TaskMetaInformation("TaskName", id2) {State = TaskState.InProcess, MinimalStartTicks = ticks};
            sut.AddMeta(taskMeta2, oldTaskIndexRecord : null);
            var taskMeta3 = new TaskMetaInformation("TaskName", id3) {State = TaskState.New, MinimalStartTicks = ticks - 5};
            sut.AddMeta(taskMeta3, oldTaskIndexRecord : null);
            var taskMeta4 = new TaskMetaInformation("TaskName", id4) {State = TaskState.Unknown, MinimalStartTicks = ticks + 1};
            sut.AddMeta(taskMeta4, oldTaskIndexRecord : null);
            var toTicks = ticks + 9;
            var taskIndexShardKeys = new[] {TaskIndexShardKey("TaskName", TaskState.InProcess), TaskIndexShardKey("TaskName", TaskState.New)};
            Assert.That(sut.GetIndexRecords(toTicks, taskIndexShardKeys).Select(x => x.TaskId).ToArray(), Is.EquivalentTo(new[] {id3, id2}));
            Assert.That(sut.GetIndexRecords(toTicks, taskIndexShardKeys.Reverse().ToArray()).Select(x => x.TaskId).ToArray(), Is.EquivalentTo(new[] {id3, id2}));
        }

        [Test]
        public void TaskWithSameIdsTest()
        {
            var ticks = Timestamp.Now.Ticks;
            var id = NewTaskId();
            var taskMeta1 = new TaskMetaInformation("TaskName", id) {State = TaskState.New, MinimalStartTicks = ticks + 10};
            sut.AddMeta(taskMeta1, oldTaskIndexRecord : null);
            var taskMeta2 = new TaskMetaInformation("TaskName", id) {State = TaskState.InProcess, MinimalStartTicks = ticks + 15};
            sut.AddMeta(taskMeta2, oldTaskIndexRecord : null);
            var newTasks = sut.GetIndexRecords(ticks + 12, new[] {TaskIndexShardKey("TaskName", TaskState.New)});
            Assert.AreEqual(1, newTasks.Length);
            Assert.AreEqual(id, newTasks[0].TaskId);
            var inProcessTasks = sut.GetIndexRecords(ticks + 12, new[] {TaskIndexShardKey("TaskName", TaskState.InProcess)});
            Assert.AreEqual(0, inProcessTasks.Length);
            inProcessTasks = sut.GetIndexRecords(ticks + 16, new[] {TaskIndexShardKey("TaskName", TaskState.InProcess)});
            Assert.AreEqual(1, inProcessTasks.Length);
            Assert.AreEqual(id, inProcessTasks[0].TaskId);
        }

        private DummyTaskDataRegistry taskDataRegistry;
        private HandleTasksMetaStorage sut;

        [IgnoredImplementation]
        private class DummyTaskDataRegistry : IRtqTaskDataRegistry
        {
            public string[] GetAllTaskNames()
            {
                throw new NotImplementedException();
            }

            public string GetTaskName(Type type)
            {
                throw new NotImplementedException();
            }

            public Type GetTaskType(string taskName)
            {
                throw new NotImplementedException();
            }

            public bool TryGetTaskType(string taskName, out Type taskType)
            {
                throw new NotImplementedException();
            }

            public string[] GetAllTaskTopics()
            {
                throw new NotImplementedException();
            }

            public string GetTaskTopic(string taskName)
            {
                return taskName;
            }
        }
    }
}