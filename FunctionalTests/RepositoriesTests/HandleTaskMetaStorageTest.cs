using System;
using System.Linq;

using GroboContainer.Infection;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Configuration;

using SKBKontur.Catalogue.Objects.TimeBasedUuid;

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
            var meta = new TaskMetaInformation("TaskName", NewTaskId())
                {
                    State = TaskState.New,
                    MinimalStartTicks = nowTicks + 1
                };
            for(var i = 0; i <= 1000; i++)
            {
                meta.MinimalStartTicks++;
                handleTasksMetaStorage.AddMeta(meta);
            }
            Assert.AreEqual(1, handleTasksMetaStorage.GetIndexRecords(nowTicks + 1002, TaskIndexShardKey("TaskName", TaskState.New)).ToArray().Length);
        }

        [Test]
        public void StressTest2()
        {
            var nowTicks = DateTime.UtcNow.Ticks;
            var metas = new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.Select(x => new TaskMetaInformation("TaskName", NewTaskId())
                {
                    State = TaskState.New,
                    MinimalStartTicks = nowTicks + x
                }).ToArray();

            for(var i = 0; i <= 100; i++)
            {
                foreach(var t in metas)
                {
                    t.MinimalStartTicks++;
                    t.State = i % 2 == 0 ? TaskState.Finished : TaskState.New;
                    handleTasksMetaStorage.AddMeta(t);
                }
            }
            Assert.AreEqual(10, handleTasksMetaStorage.GetIndexRecords(nowTicks + 1012, TaskIndexShardKey("TaskName", TaskState.Finished)).ToArray().Length);
        }

        [Test]
        public void SimpleTest()
        {
            var ticks = DateTime.UtcNow.Ticks;
            var id = NewTaskId();
            handleTasksMetaStorage.AddMeta(new TaskMetaInformation("TaskName", id)
                {
                    State = TaskState.New,
                    MinimalStartTicks = ticks
                });
            var tasks = handleTasksMetaStorage.GetIndexRecords(ticks + 1, TaskIndexShardKey("TaskName", TaskState.New)).ToArray();
            Assert.AreEqual(1, tasks.Length);
            Assert.AreEqual(id, tasks[0].TaskId);
            tasks = handleTasksMetaStorage.GetIndexRecords(ticks, TaskIndexShardKey("TaskName", TaskState.New)).ToArray();
            Assert.AreEqual(1, tasks.Length);
            Assert.AreEqual(id, tasks[0].TaskId);
            tasks = handleTasksMetaStorage.GetIndexRecords(ticks - 1, TaskIndexShardKey("TaskName", TaskState.New)).ToArray();
            Assert.AreEqual(0, tasks.Length);
        }

        [Test]
        public void DifferentStatesTest()
        {
            var ticks = DateTime.UtcNow.Ticks;
            handleTasksMetaStorage.AddMeta(new TaskMetaInformation("TaskName", NewTaskId())
                {
                    State = TaskState.InProcess,
                    MinimalStartTicks = ticks
                });
            handleTasksMetaStorage.AddMeta(new TaskMetaInformation("TaskName", NewTaskId())
                {
                    State = TaskState.Finished,
                    MinimalStartTicks = ticks
                });
            var tasks = handleTasksMetaStorage.GetIndexRecords(ticks + 1, TaskIndexShardKey("TaskName", TaskState.InProcess)).ToArray();
            Assert.AreEqual(1, tasks.Length);
        }

        [Test]
        public void DifferentNamesTest()
        {
            var ticks = DateTime.UtcNow.Ticks;
            handleTasksMetaStorage.AddMeta(new TaskMetaInformation("TaskName1", NewTaskId())
                {
                    State = TaskState.New,
                    MinimalStartTicks = ticks
                });
            handleTasksMetaStorage.AddMeta(new TaskMetaInformation("TaskName2", NewTaskId())
                {
                    State = TaskState.New,
                    MinimalStartTicks = ticks
                });
            var tasks = handleTasksMetaStorage.GetIndexRecords(ticks + 1, TaskIndexShardKey("TaskName1", TaskState.New)).ToArray();
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
            handleTasksMetaStorage.AddMeta(new TaskMetaInformation("TaskName", id1)
                {
                    State = TaskState.New,
                    MinimalStartTicks = ticks + 10
                });
            handleTasksMetaStorage.AddMeta(new TaskMetaInformation("TaskName", id2)
                {
                    State = TaskState.InProcess,
                    MinimalStartTicks = ticks
                });
            handleTasksMetaStorage.AddMeta(new TaskMetaInformation("TaskName", id3)
                {
                    State = TaskState.New,
                    MinimalStartTicks = ticks - 5
                });
            handleTasksMetaStorage.AddMeta(new TaskMetaInformation("TaskName", id4)
                {
                    State = TaskState.Unknown,
                    MinimalStartTicks = ticks + 1
                });
            var toTicks = ticks + 9;
            var taskIndexShardKeys = new[] {TaskIndexShardKey("TaskName", TaskState.InProcess), TaskIndexShardKey("TaskName", TaskState.New)};
            Assert.That(handleTasksMetaStorage.GetIndexRecords(toTicks, taskIndexShardKeys).Select(x => x.TaskId).ToArray(), Is.EqualTo(new[] {id3, id2}));
            Assert.That(handleTasksMetaStorage.GetIndexRecords(toTicks, taskIndexShardKeys.Reverse().ToArray()).Select(x => x.TaskId).ToArray(), Is.EqualTo(new[] {id3, id2}));
        }

        [Test]
        public void TaskWithSameIdsTest()
        {
            var ticks = DateTime.UtcNow.Ticks;
            var id = NewTaskId();
            handleTasksMetaStorage.AddMeta(new TaskMetaInformation("TaskName", id)
                {
                    State = TaskState.New,
                    MinimalStartTicks = ticks + 10
                });
            handleTasksMetaStorage.AddMeta(new TaskMetaInformation("TaskName", id)
                {
                    State = TaskState.InProcess,
                    MinimalStartTicks = ticks + 15
                });
            var newTasks = handleTasksMetaStorage.GetIndexRecords(ticks + 12, TaskIndexShardKey("TaskName", TaskState.New)).ToArray();
            Assert.AreEqual(1, newTasks.Length);
            Assert.AreEqual(id, newTasks[0].TaskId);
            var inProcessTasks = handleTasksMetaStorage.GetIndexRecords(ticks + 12, TaskIndexShardKey("TaskName", TaskState.InProcess)).ToArray();
            Assert.AreEqual(0, inProcessTasks.Length);
            inProcessTasks = handleTasksMetaStorage.GetIndexRecords(ticks + 16, TaskIndexShardKey("TaskName", TaskState.InProcess)).ToArray();
            Assert.AreEqual(1, inProcessTasks.Length);
            Assert.AreEqual(id, inProcessTasks[0].TaskId);
        }

        private ITaskDataRegistry taskDataRegistry;
        private IHandleTasksMetaStorage handleTasksMetaStorage;

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