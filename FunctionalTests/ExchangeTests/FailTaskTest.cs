using System;
using System.Linq;
using System.Threading;

using ExchangeService.Exceptions;
using ExchangeService.UserClasses;

using GroBuf;
using GroBuf.DataMembersExtracters;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Cassandra.Repositories.Indexes.ChildTaskIndex;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using RemoteQueue.Handling;
using RemoteQueue.Profiling;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock;
using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock.RemoteLocker;
using SKBKontur.Catalogue.CassandraPrimitives.Storages.Primitives;
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
            var serializer = new Serializer(new AllPropertiesExtractor());
            var ticksHolder = new TicksHolder(serializer, parameters);
            var globalTime = new GlobalTime(ticksHolder);
            var taskDataBlobStorage = new TaskDataBlobStorage(parameters, serializer, globalTime);
            var taskMinimalStartTicksIndex = new TaskMinimalStartTicksIndex(parameters, ticksHolder, serializer, globalTime);
            var eventLongRepository = new EventLogRepository(serializer, globalTime, parameters, ticksHolder);
            var taskMetaInformationBlobStorage = new TaskMetaInformationBlobStorage(parameters, serializer, globalTime);
            var childTaskIndex = new ChildTaskIndex(parameters, serializer, taskMetaInformationBlobStorage);
            var handleTasksMetaStorage = new HandleTasksMetaStorage(taskMetaInformationBlobStorage, taskMinimalStartTicksIndex, eventLongRepository, globalTime, childTaskIndex);
            handleTaskCollection = new HandleTaskCollection(handleTasksMetaStorage, taskDataBlobStorage, new EmptyRemoteTaskQueueProfiler());
            var remoteLockImplementationSettings = CassandraRemoteLockImplementationSettings.Default(new ColumnFamilyFullName(parameters.Settings.QueueKeyspace, parameters.LockColumnFamilyName));
            var remoteLockCreator = new RemoteLocker(new CassandraRemoteLockImplementation(parameters.CassandraCluster, serializer, remoteLockImplementationSettings), new RemoteLockerMetrics(parameters.Settings.QueueKeyspace));
            testCounterRepository = new TestCounterRepository(new TestCassandraCounterBlobRepository(parameters, serializer, globalTime), remoteLockCreator);
            taskQueue = Container.Get<IRemoteTaskQueue>();
        }

        [Test]
        public void TestTooLateOneFailTask()
        {
            var taskId = AddTask(9);
            try
            {
                Wait(new[] {taskId}, 12345);
                throw new Exception("Wait не должен отработать");
            }
            catch(TooLateException)
            {
                var count = testCounterRepository.GetCounter(taskId);
                Assert.That(count < 9 && count >= 1);
            }
        }

        [Test]
        public void TestManyFails()
        {
            const int count = 5;
            var ids = new string[count];
            for (var i = 0; i < count; i++)
                ids[i] = AddTask(7);
            Wait(ids, 60000);
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
                    if(task.Meta.State != TaskState.Fatal)
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
                    Container.CheckTaskMinimalStartTicksIndexStates(taskIds.ToDictionary(s => s, s => TaskState.Fatal));
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