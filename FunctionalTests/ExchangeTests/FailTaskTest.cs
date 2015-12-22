using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using ExchangeService.Exceptions;
using ExchangeService.UserClasses;

using GroBuf;
using GroBuf.DataMembersExtracters;

using MoreLinq;

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
using SKBKontur.Catalogue.ServiceLib.Logging;

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
            var taskMinimalStartTicksIndex = new TaskMinimalStartTicksIndex(parameters, serializer, globalTime, new OldestLiveRecordTicksHolder(ticksHolder));
            var eventLongRepository = new EventLogRepository(serializer, globalTime, parameters, ticksHolder);
            var taskMetaInformationBlobStorage = new TaskMetaInformationBlobStorage(parameters, serializer, globalTime);
            var childTaskIndex = new ChildTaskIndex(parameters, serializer, taskMetaInformationBlobStorage);
            var handleTasksMetaStorage = new HandleTasksMetaStorage(taskMetaInformationBlobStorage, taskMinimalStartTicksIndex, eventLongRepository, globalTime, childTaskIndex, Container.Get<ITaskDataRegistry>());
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
                Wait(new[] {taskId}, 1000);
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

        [Test]
        [Repeat(10)]
        public void TestStressManyTasks()
        {
            const int count = 100;
            var ids = new string[count];
            for(var j = 0; j < count; j++)
                ids[j] = AddTask(10);
            Wait(ids, 90000);
        }

        private string AddTask(int attempts)
        {
            var task = taskQueue.CreateTask(new FakeFailTaskData());
            testCounterRepository.SetValueForCounter(task.Id, attempts);
            task.Queue();
            return task.Id;
        }

        private void Wait(string[] taskIds, int timeout = 5000)
        {
            var sw = Stopwatch.StartNew();
            var sleepInterval = Math.Max(500, timeout / 10);
            while(true)
            {
                var allTasksAreFinished = handleTaskCollection.GetTasks(taskIds).All(x => x.Meta.State == TaskState.Fatal);
                var attempts = taskIds.Select(testCounterRepository.GetCounter).ToArray();
                Log.For(this).InfoFormat("CurrentCounterValues: {0}", string.Join(", ", attempts));
                var notFinishedTaskIds = taskIds.EquiZip(attempts, (taskId, attempt) => new {taskId, attempt}).Where(x => x.attempt > 0).Select(x => x.taskId).ToArray();
                if(allTasksAreFinished)
                {
                    Assert.That(notFinishedTaskIds, Is.Empty);
                    Container.CheckTaskMinimalStartTicksIndexStates(taskIds.ToDictionary(s => s, s => TaskIndexShardKey("FakeFailTaskData", TaskState.Fatal)));
                    break;
                }
                if(sw.ElapsedMilliseconds > timeout)
                    throw new TooLateException("Время ожидания превысило {0} мс. NotFinihedTaskIds: {1}", timeout, string.Join(", ", notFinishedTaskIds));
                Thread.Sleep(sleepInterval);
            }
        }

        private IHandleTaskCollection handleTaskCollection;
        private ITestCounterRepository testCounterRepository;
        private IRemoteTaskQueue taskQueue;
    }
}