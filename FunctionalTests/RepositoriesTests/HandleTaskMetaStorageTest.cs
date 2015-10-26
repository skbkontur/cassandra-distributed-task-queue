using System;
using System.Linq;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Handling;

namespace FunctionalTests.RepositoriesTests
{
    public class HandleTaskMetaStorageTest : FunctionalTestBaseWithoutServices
    {
        public override void SetUp()
        {
            base.SetUp();
            taskTopicResolver = new DummyTaskTopicResolver();
            Container.Configurator.ForAbstraction<ITaskTopicResolver>().UseInstances(taskTopicResolver);
            handleTasksMetaStorage = Container.Get<IHandleTasksMetaStorage>();
        }

        private TaskTopicAndState TaskTopicAndState(string taskName, TaskState taskState)
        {
            return new TaskTopicAndState(taskTopicResolver.GetTaskTopic(taskName), taskState);
        }

        [Test]
        public void StressTest()
        {
            var nowTicks = DateTime.UtcNow.Ticks;
            var meta = new TaskMetaInformation("TaskName", Guid.NewGuid().ToString())
                {
                    State = TaskState.New,
                    MinimalStartTicks = nowTicks + 1
                };
            for(var i = 0; i <= 1000; i++)
            {
                meta.MinimalStartTicks++;
                handleTasksMetaStorage.AddMeta(meta);
            }
            Assert.AreEqual(1, handleTasksMetaStorage.GetIndexRecords(nowTicks + 1002, TaskTopicAndState("TaskName", TaskState.New)).ToArray().Length);
        }

        [Test]
        public void StressTest2()
        {
            var nowTicks = DateTime.UtcNow.Ticks;
            var metas = new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.Select(x => new TaskMetaInformation("TaskName", Guid.NewGuid().ToString())
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
            Assert.AreEqual(10, handleTasksMetaStorage.GetIndexRecords(nowTicks + 1012, TaskTopicAndState("TaskName", TaskState.Finished)).ToArray().Length);
        }

        [Test]
        public void SimpleTest()
        {
            var ticks = DateTime.UtcNow.Ticks;
            var id = Guid.NewGuid().ToString();
            handleTasksMetaStorage.AddMeta(new TaskMetaInformation("TaskName", id)
                {
                    State = TaskState.New,
                    MinimalStartTicks = ticks
                });
            var tasks = handleTasksMetaStorage.GetIndexRecords(ticks + 1, TaskTopicAndState("TaskName", TaskState.New)).ToArray();
            Assert.AreEqual(1, tasks.Length);
            Assert.AreEqual(id, tasks[0].TaskId);
            tasks = handleTasksMetaStorage.GetIndexRecords(ticks, TaskTopicAndState("TaskName", TaskState.New)).ToArray();
            Assert.AreEqual(1, tasks.Length);
            Assert.AreEqual(id, tasks[0].TaskId);
            tasks = handleTasksMetaStorage.GetIndexRecords(ticks - 1, TaskTopicAndState("TaskName", TaskState.New)).ToArray();
            Assert.AreEqual(0, tasks.Length);
        }

        [Test]
        public void DifferentStatesTest()
        {
            var ticks = DateTime.UtcNow.Ticks;
            handleTasksMetaStorage.AddMeta(new TaskMetaInformation("TaskName", Guid.NewGuid().ToString())
                {
                    State = TaskState.InProcess,
                    MinimalStartTicks = ticks
                });
            handleTasksMetaStorage.AddMeta(new TaskMetaInformation("TaskName", Guid.NewGuid().ToString())
                {
                    State = TaskState.Finished,
                    MinimalStartTicks = ticks
                });
            var tasks = handleTasksMetaStorage.GetIndexRecords(ticks + 1, TaskTopicAndState("TaskName", TaskState.InProcess)).ToArray();
            Assert.AreEqual(1, tasks.Length);
        }

        [Test]
        public void DifferentNamesTest()
        {
            var ticks = DateTime.UtcNow.Ticks;
            handleTasksMetaStorage.AddMeta(new TaskMetaInformation("TaskName1", Guid.NewGuid().ToString())
                {
                    State = TaskState.New,
                    MinimalStartTicks = ticks
                });
            handleTasksMetaStorage.AddMeta(new TaskMetaInformation("TaskName2", Guid.NewGuid().ToString())
                {
                    State = TaskState.New,
                    MinimalStartTicks = ticks
                });
            var tasks = handleTasksMetaStorage.GetIndexRecords(ticks + 1, TaskTopicAndState("TaskName1", TaskState.New)).ToArray();
            Assert.AreEqual(1, tasks.Length);
        }

        [Test]
        public void ManyTasksTest()
        {
            var ticks = DateTime.UtcNow.Ticks;
            var id1 = Guid.NewGuid().ToString();
            var id2 = Guid.NewGuid().ToString();
            var id3 = Guid.NewGuid().ToString();
            var id4 = Guid.NewGuid().ToString();
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
            var tasks = handleTasksMetaStorage.GetIndexRecords(ticks + 9, TaskTopicAndState("TaskName", TaskState.InProcess), TaskTopicAndState("TaskName", TaskState.New)).ToArray();
            Assert.AreEqual(2, tasks.Length);
            Assert.AreEqual(id2, tasks[0].TaskId);
            Assert.AreEqual(id3, tasks[1].TaskId);
        }

        [Test]
        public void TaskWithSameIdsTest()
        {
            var ticks = DateTime.UtcNow.Ticks;
            var id = Guid.NewGuid().ToString();
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
            var newTasks = handleTasksMetaStorage.GetIndexRecords(ticks + 12, TaskTopicAndState("TaskName", TaskState.New)).ToArray();
            Assert.AreEqual(1, newTasks.Length);
            Assert.AreEqual(id, newTasks[0].TaskId);
            var inProcessTasks = handleTasksMetaStorage.GetIndexRecords(ticks + 12, TaskTopicAndState("TaskName", TaskState.InProcess)).ToArray();
            Assert.AreEqual(0, inProcessTasks.Length);
            inProcessTasks = handleTasksMetaStorage.GetIndexRecords(ticks + 16, TaskTopicAndState("TaskName", TaskState.InProcess)).ToArray();
            Assert.AreEqual(1, inProcessTasks.Length);
            Assert.AreEqual(id, inProcessTasks[0].TaskId);
        }

        private ITaskTopicResolver taskTopicResolver;
        private IHandleTasksMetaStorage handleTasksMetaStorage;

        private class DummyTaskTopicResolver : ITaskTopicResolver
        {
            public string[] GetAllTaskTopics()
            {
                return new string[0];
            }

            public string GetTaskTopic(string taskName)
            {
                return taskName;
            }
        }
    }
}