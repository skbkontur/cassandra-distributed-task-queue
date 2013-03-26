using System;
using System.Linq;
using System.Threading;

using ExchangeService.Exceptions;
using ExchangeService.UserClasses;

using GroBuf;

using NUnit.Framework;

using RemoteLock;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using RemoteQueue.Handling;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas;

namespace FunctionalTests.ExchangeTests
{
    public class FailTaskTest : FunctionalTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            var cassandraSettings = Container.Get<ICassandraSettings>();
            var cassandraCluster = Container.Get<ICassandraCluster>();
            var parameters = new ColumnFamilyRepositoryParameters(cassandraCluster, cassandraSettings);
            var serializer = new Serializer();
            var ticksHolder = new TicksHolder(serializer, parameters);
            var globalTime = new GlobalTime(ticksHolder);
            var taskDataBlobStorage = new TaskDataBlobStorage(parameters, serializer, globalTime);
            var taskMinimalStartTicksIndex = new TaskMinimalStartTicksIndex(parameters, ticksHolder, serializer, globalTime, cassandraSettings);
            var eventLongRepository = new EventLogRepository(serializer, globalTime, parameters, ticksHolder);
            var handleTasksMetaStorage = new HandleTasksMetaStorage(new TaskMetaInformationBlobStorage(parameters, serializer, globalTime), taskMinimalStartTicksIndex, eventLongRepository, globalTime);
            handleTaskCollection = new HandleTaskCollection(handleTasksMetaStorage, taskDataBlobStorage);
            testCounterRepository = new TestCounterRepository(new TestCassandraCounterBlobRepository(parameters, serializer, globalTime),
                                                              new RemoteLockCreator(new CassandraRemoteLockImplementation(parameters.CassandraCluster, parameters.Settings, serializer, parameters.Settings.QueueKeyspace, parameters.LockColumnFamilyName)));
            taskQueue = Container.Get<IRemoteTaskQueue>();
        }

        [Test]
        public void TestTooLateOneFailTask()
        {
            var taskId = AddTask(8);
            try
            {
                Wait(new[] {taskId}, 12345);
                throw new Exception("Wait не должен отработать");
            }
            catch(TooLateException)
            {
                Assert.AreEqual(1, testCounterRepository.GetCounter(taskId));
            }
        }

        [Test]
        public void TestOneFailTask()
        {
            var taskId = AddTask(3);
            Wait(new[] {taskId});
        }

        [Test]
        public void TestManyFailTask()
        {
            const int count = 2;
            var ids = new string[count];
            for(var i = 0; i < count; i++)
                ids[i] = AddTask(3);
            Wait(ids, 15000);
        }

        [Test, Ignore("Стресс-тест")]
        public void TestStress()
        {
            for(var i = 0; i < 1000; i++)
                TestManyFailTask();
        }

        [Test, Ignore("Стресс-тест-много-задач")]
        public void TestStressManyTasks()
        {
            for(var i = 0; i < 10; i++)
            {
                const int count = 200;
                var ids = new string[count];
                for(var j = 0; j < count; j++)
                    ids[j] = AddTask(15);
                Wait(ids, 90000);
            }
        }

        private string AddTask(int attempts)
        {
            var task = taskQueue.CreateTask(new FakeFailTaskData());
            testCounterRepository.SetValueForCounter(task.Id, attempts);
            task.Queue();
            return task.Id;
        }

        private void Wait(string[] taskIds, int ms = 5000)
        {
            var current = 0;
            while(true)
            {
                var fail = false;
                for(var i = 0; i < taskIds.Length; i++)
                {
                    var task = handleTaskCollection.GetTask(taskIds[i]);
                    if(task.Meta.State != TaskState.Finished)
                        fail = true;
                }

                var attempts = taskIds.Select(testCounterRepository.GetCounter).ToArray();
                Console.WriteLine(Now() + " CurrentValues: " + String.Join(", ", attempts));
                if(!fail)
                {
                    for(var i = 0; i < attempts.Length; i++)
                    {
                        var attempt = attempts[i];
                        if(attempt != 0)
                            Console.WriteLine(taskIds[i]);
                        Assert.AreEqual(0, attempt);
                    }
                    break;
                }

                Thread.Sleep(sleepInterval);
                current += sleepInterval;
                if(current > ms)
                    throw new TooLateException("Время ожидания превысило {0} мс.", ms);
            }
        }

        private static string Now()
        {
            return DateTime.UtcNow.ToString("dd.MM.yyyy mm:hh:ss.ffff");
        }

        private IHandleTaskCollection handleTaskCollection;
        private ITestCounterRepository testCounterRepository;
        private IRemoteTaskQueue taskQueue;
        private const int sleepInterval = 200;
    }
}