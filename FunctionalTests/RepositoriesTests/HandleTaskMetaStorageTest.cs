using System;
using System.Linq;

using GroboContainer.Core;
using GroboContainer.Infection;

using GroBuf;
using GroBuf.DataMembersExtracters;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Configuration;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.Objects.TimeBasedUuid;
using SKBKontur.Catalogue.RemoteTaskQueue.Common;
using SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue;

namespace FunctionalTests.RepositoriesTests
{
    public class HandleTaskMetaStorageTest : FunctionalTestBaseWithoutServices
    {
        public override void SetUp()
        {
            base.SetUp();
            taskDataRegistry = new DummyTaskDataRegistry();
            Container.Configurator.ForAbstraction<ITaskDataRegistry>().UseInstances(taskDataRegistry);
            handleTasksMetaStorage = Container.Get<IHandleTasksMetaStorage>();
        }

        protected override void ConfigureContainer(Container container)
        {
            container.Configurator.ForAbstraction<ISerializer>().UseInstances(new Serializer(new AllPropertiesExtractor(), null, GroBufOptions.MergeOnRead));
            var remoteQueueTestsCassandraSettings = new RemoteQueueTestsCassandraSettings();
            container.Configurator.ForAbstraction<ICassandraClusterSettings>().UseInstances(remoteQueueTestsCassandraSettings);
            container.Configurator.ForAbstraction<IRemoteTaskQueueSettings>().UseInstances(remoteQueueTestsCassandraSettings);
            container.ConfigureLockRepository();
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
            var nowTicks = DateTime.UtcNow.Ticks;
            var meta = new TaskMetaInformation("TaskName", NewTaskId()) {State = TaskState.New};
            meta.SetMinimalStartTicks(new Timestamp(nowTicks + 1), defaultTtl);
            for(var i = 0; i <= 1000; i++)
            {
                var oldTaskIndexRecord = handleTasksMetaStorage.FormatIndexRecord(meta);
                meta.MinimalStartTicks++;
                handleTasksMetaStorage.AddMeta(meta, oldTaskIndexRecord);
            }
            Assert.AreEqual(1, handleTasksMetaStorage.GetIndexRecords(nowTicks + 1002, new[] {TaskIndexShardKey("TaskName", TaskState.New)}).Length);
        }

        [Test]
        public void StressTest2()
        {
            var nowTicks = DateTime.UtcNow.Ticks;
            var metas = new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.Select(x =>
                {
                    var taskMeta = new TaskMetaInformation("TaskName", NewTaskId()) {State = TaskState.New};
                    taskMeta.SetMinimalStartTicks(new Timestamp(nowTicks + x), defaultTtl);
                    return taskMeta;
                }).ToArray();
            for(var i = 0; i <= 100; i++)
            {
                foreach(var t in metas)
                {
                    var oldTaskIndexRecord = handleTasksMetaStorage.FormatIndexRecord(t);
                    t.MinimalStartTicks++;
                    t.State = i % 2 == 0 ? TaskState.Finished : TaskState.New;
                    handleTasksMetaStorage.AddMeta(t, oldTaskIndexRecord);
                }
            }
            Assert.AreEqual(10, handleTasksMetaStorage.GetIndexRecords(nowTicks + 1012, new[] {TaskIndexShardKey("TaskName", TaskState.Finished)}).Length);
        }

        [Test]
        public void SimpleTest()
        {
            var ticks = DateTime.UtcNow.Ticks;
            var id = NewTaskId();
            var taskMeta = new TaskMetaInformation("TaskName", id) {State = TaskState.New};
            taskMeta.SetMinimalStartTicks(new Timestamp(ticks), defaultTtl);
            handleTasksMetaStorage.AddMeta(taskMeta, oldTaskIndexRecord : null);
            var tasks = handleTasksMetaStorage.GetIndexRecords(ticks + 1, new[] {TaskIndexShardKey("TaskName", TaskState.New)});
            Assert.AreEqual(1, tasks.Length);
            Assert.AreEqual(id, tasks[0].TaskId);
            tasks = handleTasksMetaStorage.GetIndexRecords(ticks, new[] {TaskIndexShardKey("TaskName", TaskState.New)});
            Assert.AreEqual(1, tasks.Length);
            Assert.AreEqual(id, tasks[0].TaskId);
            tasks = handleTasksMetaStorage.GetIndexRecords(ticks - 1, new[] {TaskIndexShardKey("TaskName", TaskState.New)});
            Assert.AreEqual(0, tasks.Length);
        }

        [Test]
        public void DifferentStatesTest()
        {
            var ticks = DateTime.UtcNow.Ticks;
            var taskMeta1 = new TaskMetaInformation("TaskName", NewTaskId()) {State = TaskState.InProcess};
            taskMeta1.SetMinimalStartTicks(new Timestamp(ticks), defaultTtl);
            handleTasksMetaStorage.AddMeta(taskMeta1, oldTaskIndexRecord : null);
            var taskMeta2 = new TaskMetaInformation("TaskName", NewTaskId()) {State = TaskState.Finished};
            taskMeta2.SetMinimalStartTicks(new Timestamp(ticks), defaultTtl);
            handleTasksMetaStorage.AddMeta(taskMeta2, oldTaskIndexRecord : null);
            var tasks = handleTasksMetaStorage.GetIndexRecords(ticks + 1, new[] {TaskIndexShardKey("TaskName", TaskState.InProcess)});
            Assert.AreEqual(1, tasks.Length);
        }

        [Test]
        public void DifferentNamesTest()
        {
            var ticks = DateTime.UtcNow.Ticks;
            var taskMeta1 = new TaskMetaInformation("TaskName1", NewTaskId()) {State = TaskState.New};
            taskMeta1.SetMinimalStartTicks(new Timestamp(ticks), defaultTtl);
            handleTasksMetaStorage.AddMeta(taskMeta1, oldTaskIndexRecord : null);
            var taskMeta2 = new TaskMetaInformation("TaskName2", NewTaskId()) {State = TaskState.New};
            taskMeta2.SetMinimalStartTicks(new Timestamp(ticks), defaultTtl);
            handleTasksMetaStorage.AddMeta(taskMeta2, oldTaskIndexRecord : null);
            var tasks = handleTasksMetaStorage.GetIndexRecords(ticks + 1, new[] {TaskIndexShardKey("TaskName1", TaskState.New)});
            Assert.AreEqual(1, tasks.Length);
        }

        [Test]
        public void ManyTasksTest()
        {
            var ticks = DateTime.UtcNow.Ticks;
            var id1 = NewTaskId();
            var id2 = NewTaskId();
            var id3 = NewTaskId();
            var id4 = NewTaskId();
            var taskMeta1 = new TaskMetaInformation("TaskName", id1) {State = TaskState.New};
            taskMeta1.SetMinimalStartTicks(new Timestamp(ticks + 10), defaultTtl);
            handleTasksMetaStorage.AddMeta(taskMeta1, oldTaskIndexRecord : null);
            var taskMeta2 = new TaskMetaInformation("TaskName", id2) {State = TaskState.InProcess};
            taskMeta2.SetMinimalStartTicks(new Timestamp(ticks), defaultTtl);
            handleTasksMetaStorage.AddMeta(taskMeta2, oldTaskIndexRecord : null);
            var taskMeta3 = new TaskMetaInformation("TaskName", id3) {State = TaskState.New};
            taskMeta3.SetMinimalStartTicks(new Timestamp(ticks - 5), defaultTtl);
            handleTasksMetaStorage.AddMeta(taskMeta3, oldTaskIndexRecord : null);
            var taskMeta4 = new TaskMetaInformation("TaskName", id4) {State = TaskState.Unknown};
            taskMeta4.SetMinimalStartTicks(new Timestamp(ticks + 1), defaultTtl);
            handleTasksMetaStorage.AddMeta(taskMeta4, oldTaskIndexRecord : null);
            var toTicks = ticks + 9;
            var taskIndexShardKeys = new[] {TaskIndexShardKey("TaskName", TaskState.InProcess), TaskIndexShardKey("TaskName", TaskState.New)};
            Assert.That(handleTasksMetaStorage.GetIndexRecords(toTicks, taskIndexShardKeys).Select(x => x.TaskId).ToArray(), Is.EquivalentTo(new[] {id3, id2}));
            Assert.That(handleTasksMetaStorage.GetIndexRecords(toTicks, taskIndexShardKeys.Reverse().ToArray()).Select(x => x.TaskId).ToArray(), Is.EquivalentTo(new[] {id3, id2}));
        }

        [Test]
        public void TaskWithSameIdsTest()
        {
            var ticks = DateTime.UtcNow.Ticks;
            var id = NewTaskId();
            var taskMeta1 = new TaskMetaInformation("TaskName", id) {State = TaskState.New};
            taskMeta1.SetMinimalStartTicks(new Timestamp(ticks + 10), defaultTtl);
            handleTasksMetaStorage.AddMeta(taskMeta1, oldTaskIndexRecord : null);
            var taskMeta2 = new TaskMetaInformation("TaskName", id) {State = TaskState.InProcess};
            taskMeta2.SetMinimalStartTicks(new Timestamp(ticks + 15), defaultTtl);
            handleTasksMetaStorage.AddMeta(taskMeta2, oldTaskIndexRecord : null);
            var newTasks = handleTasksMetaStorage.GetIndexRecords(ticks + 12, new[] {TaskIndexShardKey("TaskName", TaskState.New)});
            Assert.AreEqual(1, newTasks.Length);
            Assert.AreEqual(id, newTasks[0].TaskId);
            var inProcessTasks = handleTasksMetaStorage.GetIndexRecords(ticks + 12, new[] {TaskIndexShardKey("TaskName", TaskState.InProcess)});
            Assert.AreEqual(0, inProcessTasks.Length);
            inProcessTasks = handleTasksMetaStorage.GetIndexRecords(ticks + 16, new[] {TaskIndexShardKey("TaskName", TaskState.InProcess)});
            Assert.AreEqual(1, inProcessTasks.Length);
            Assert.AreEqual(id, inProcessTasks[0].TaskId);
        }

        private ITaskDataRegistry taskDataRegistry;
        private IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly TimeSpan defaultTtl = TimeSpan.FromHours(1);

        [IgnoredImplementation]
        private class DummyTaskDataRegistry : ITaskDataRegistry
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