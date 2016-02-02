using System;

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
using RemoteQueue.Configuration;
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
    public class FailTaskTest : TasksWithCounterTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            var cassandraSettings = Container.Get<ICassandraSettings>();
            var cassandraCluster = Container.Get<ICassandraCluster>();
            var parameters = new ColumnFamilyRepositoryParameters(cassandraCluster, cassandraSettings);
            var serializer = new Serializer(new AllPropertiesExtractor());
            var ticksHolder = new TicksHolder(cassandraCluster, serializer, cassandraSettings);
            var globalTime = new GlobalTime(ticksHolder);
            var taskDataStorage = new TaskDataStorage(cassandraCluster, serializer, cassandraSettings);
            var taskMinimalStartTicksIndex = new TaskMinimalStartTicksIndex(cassandraCluster, serializer, cassandraSettings, new OldestLiveRecordTicksHolder(ticksHolder));
            var eventLongRepository = new EventLogRepository(serializer, globalTime, parameters, ticksHolder);
            var taskMetaStorage = new TaskMetaStorage(cassandraCluster, serializer, cassandraSettings);
            var childTaskIndex = new ChildTaskIndex(parameters, serializer, taskMetaStorage);
            var handleTasksMetaStorage = new HandleTasksMetaStorage(taskMetaStorage, taskMinimalStartTicksIndex, eventLongRepository, globalTime, childTaskIndex, Container.Get<ITaskDataRegistry>());
            handleTaskCollection = new HandleTaskCollection(handleTasksMetaStorage, taskDataStorage, new EmptyRemoteTaskQueueProfiler());
            var remoteLockImplementationSettings = CassandraRemoteLockImplementationSettings.Default(new ColumnFamilyFullName(parameters.Settings.QueueKeyspace, parameters.LockColumnFamilyName));
            var remoteLockCreator = new RemoteLocker(new CassandraRemoteLockImplementation(parameters.CassandraCluster, serializer, remoteLockImplementationSettings), new RemoteLockerMetrics(parameters.Settings.QueueKeyspace));
            testCounterRepository = new TestCounterRepository(cassandraCluster, serializer, cassandraSettings, remoteLockCreator);
            taskQueue = Container.Get<IRemoteTaskQueue>();
        }

        [Test]
        public void TestTooLateOneFailTask()
        {
            var taskId = AddTask(9);
            try
            {
                WaitForFatalState(new[] {taskId}, TimeSpan.FromSeconds(1));
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
            for(var i = 0; i < count; i++)
                ids[i] = AddTask(7);
            WaitForFatalState(ids, TimeSpan.FromSeconds(60));
        }

        [Test]
        public void TestOneFailTask()
        {
            var taskId = AddTask(3);
            WaitForFatalState(new[] {taskId}, TimeSpan.FromSeconds(5));
        }

        [Test]
        public void TestManyFailTask()
        {
            const int count = 2;
            var ids = new string[count];
            for(var i = 0; i < count; i++)
                ids[i] = AddTask(3);
            WaitForFatalState(ids, TimeSpan.FromSeconds(15));
        }

        [Test]
        [Repeat(10)]
        public void TestStressManyTasks()
        {
            const int count = 100;
            var ids = new string[count];
            for(var j = 0; j < count; j++)
                ids[j] = AddTask(10);
            WaitForFatalState(ids, TimeSpan.FromSeconds(90));
        }

        private string AddTask(int attempts)
        {
            var task = taskQueue.CreateTask(new FakeFailTaskData());
            testCounterRepository.SetValueForCounter(task.Id, attempts);
            task.Queue();
            return task.Id;
        }

        private void WaitForFatalState(string[] taskIds, TimeSpan timeout)
        {
            Wait(taskIds, TaskState.Fatal, "FakeFailTaskData", timeout);
        }

        private IRemoteTaskQueue taskQueue;
    }
}