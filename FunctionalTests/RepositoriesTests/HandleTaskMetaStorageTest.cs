using System;
using System.Linq;
using System.Threading;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;

namespace FunctionalTests.RepositoriesTests
{
    public class HandleTaskMetaStorageTest : FunctionalTestBaseWithoutServices
    {
        public override void SetUp()
        {
            base.SetUp();
            handleTasksMetaStorage = Container.Get<IHandleTasksMetaStorage>();
        }

        [Test, Ignore("stress")]
        public void StressTest()
        {
            var meta = new TaskMetaInformation
                {
                    Id = Guid.NewGuid().ToString(),
                    State = TaskState.New,
                    MinimalStartTicks = 1
                };
            for(int i = 0; i <= 1000; i++)
            {
                if(i % 10 == 0)
                    Console.WriteLine(i);
                meta.MinimalStartTicks++;
                handleTasksMetaStorage.AddMeta(meta);
            }
            Assert.AreEqual(1, handleTasksMetaStorage.GetAllTasksInStates(1020, TaskState.New).ToArray().Length);
        }

        [Test, Ignore("stress")]
        public void StressTest2()
        {
            var metas = new[]{1,2,3,4,5,6,7,8,9,10}.Select(x=>new TaskMetaInformation
            {
                Id = Guid.NewGuid().ToString(),
                State = TaskState.New,
                MinimalStartTicks = x
            }).ToArray();
            
            for (int i = 0; i <= 100; i++)
            {
                if (i % 10 == 0)
                    Console.WriteLine(i);
                foreach(var t in metas)
                {
                    t.MinimalStartTicks++;
                    t.State = i%2 == 0 ? TaskState.Finished : TaskState.New;
                    handleTasksMetaStorage.AddMeta(t);
                }
            }
            Assert.AreEqual(10, handleTasksMetaStorage.GetAllTasksInStates(1020, TaskState.Finished).ToArray().Length);
        }

        [Test]
        public void SimpleTest()
        {
            long ticks = DateTime.UtcNow.Ticks;
            string id = Guid.NewGuid().ToString();
            handleTasksMetaStorage.AddMeta(new TaskMetaInformation
                {
                    State = TaskState.New,
                    Id = id,
                    MinimalStartTicks = ticks
                });
            string[] tasks = handleTasksMetaStorage.GetAllTasksInStates(ticks + 1, TaskState.New).ToArray();
            Assert.AreEqual(1, tasks.Length);
            Assert.AreEqual(id, tasks[0]);
            tasks = handleTasksMetaStorage.GetAllTasksInStates(ticks, TaskState.New).ToArray();
            Assert.AreEqual(1, tasks.Length);
            Assert.AreEqual(id, tasks[0]);
            tasks = handleTasksMetaStorage.GetAllTasksInStates(ticks - 1, TaskState.New).ToArray();
            Assert.AreEqual(0, tasks.Length);
        }

        [Test]
        public void DifferentStatesTest()
        {
            long ticks = DateTime.UtcNow.Ticks;
            string id = Guid.NewGuid().ToString();
            handleTasksMetaStorage.AddMeta(new TaskMetaInformation
                {
                    State = TaskState.New,
                    Id = id,
                    MinimalStartTicks = ticks
                });
            string[] tasks = handleTasksMetaStorage.GetAllTasksInStates(ticks + 1, TaskState.InProcess).ToArray();
            Assert.AreEqual(0, tasks.Length);
        }

        [Test]
        public void ManyTasksTest()
        {
            long ticks = DateTime.UtcNow.Ticks;
            string id = Guid.NewGuid().ToString();
            string id2 = Guid.NewGuid().ToString();
            string id3 = Guid.NewGuid().ToString();
            string id4 = Guid.NewGuid().ToString();
            handleTasksMetaStorage.AddMeta(new TaskMetaInformation
                {
                    State = TaskState.New,
                    Id = id,
                    MinimalStartTicks = ticks + 10
                });
            handleTasksMetaStorage.AddMeta(new TaskMetaInformation
                {
                    State = TaskState.InProcess,
                    Id = id2,
                    MinimalStartTicks = ticks
                });
            handleTasksMetaStorage.AddMeta(new TaskMetaInformation
                {
                    State = TaskState.New,
                    Id = id3,
                    MinimalStartTicks = ticks - 5
                });
            handleTasksMetaStorage.AddMeta(new TaskMetaInformation
                {
                    State = TaskState.Unknown,
                    Id = id4,
                    MinimalStartTicks = ticks + 1
                });
            string[] tasks = handleTasksMetaStorage.GetAllTasksInStates(ticks + 9, TaskState.InProcess, TaskState.New).ToArray();
            Assert.AreEqual(2, tasks.Length);
            Assert.AreEqual(id2, tasks[0]);
            Assert.AreEqual(id3, tasks[1]);
        }

        [Test]
        public void TaskWithSameIdsTest()
        {
            long ticks = DateTime.UtcNow.Ticks;
            string id = Guid.NewGuid().ToString();
            handleTasksMetaStorage.AddMeta(new TaskMetaInformation
                {
                    State = TaskState.New,
                    Id = id,
                    MinimalStartTicks = ticks + 10
                });
            handleTasksMetaStorage.AddMeta(new TaskMetaInformation
                {
                    State = TaskState.InProcess,
                    Id = id,
                    MinimalStartTicks = ticks + 15
                });
            string[] newTasks = handleTasksMetaStorage.GetAllTasksInStates(ticks + 12, TaskState.New).ToArray();
            Assert.AreEqual(1, newTasks.Length);
            Assert.AreEqual(id, newTasks[0]);
            string[] inProcessTasks = handleTasksMetaStorage.GetAllTasksInStates(ticks + 12, TaskState.InProcess).ToArray();
            Assert.AreEqual(0, inProcessTasks.Length);
            inProcessTasks = handleTasksMetaStorage.GetAllTasksInStates(ticks + 16, TaskState.InProcess).ToArray();
            Assert.AreEqual(1, inProcessTasks.Length);
            Assert.AreEqual(id, inProcessTasks[0]);
        }

        private IHandleTasksMetaStorage handleTasksMetaStorage;
    }
}