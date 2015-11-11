using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using ExchangeService.UserClasses;

using FluentAssertions;

using GroboContainer.Core;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.NUnit.Extensions.CommonWrappers;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.NUnit.Extensions.TestEnvironments.Container;
using SKBKontur.Catalogue.RemoteTaskQueue.Common;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Client;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.DataTypes;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;

using TestCommon.NUnitWrappers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.FunctionalTests
{
    [EdiTestSuite, WithApplicationSettings(FileName = "functionalTests.csf"),
     WithDefaultSerializer,
     WithExchangeServices,
     WithRemoteLock(), //NOTE lock used in TestCounterRepository
     WithCassandra("CatalogueCluster", "QueueKeyspace")]
    public class TaskCounterTest
    {
        [ContainerSetUp]
        public void SetUp()
        {
            Monitoring.RestartProcessingTaskCounter(DateTime.UtcNow);
        }

        [Test]
        public void TestCounts()
        {
            container.Get<IExchangeServiceClient>().Stop();
            TaskQueue.CreateTask(new SlowTaskData() {TimeMs = 1}).Queue();
            TaskQueue.CreateTask(new SlowTaskData() {TimeMs = 1}).Queue();
            TaskCount count = null;
            WaitFor(() => (count = Monitoring.GetProcessingTaskCount()).Count == 2, TimeSpan.FromSeconds(10));
            Assert.IsNotNull(count);
            Assert.AreEqual(2, count.Counts[(int)TaskState.New]);

            container.Get<IExchangeServiceClient>().Start();
            WaitFor(() => Monitoring.GetProcessingTaskCount().Count == 0, TimeSpan.FromSeconds(10));
        }

        [Test]
        public void TestCounter()
        {
            WaitFor(() => Monitoring.GetProcessingTaskCount().Count == 0, TimeSpan.FromSeconds(10));
            var taskIds = new List<string>();
            var w = Stopwatch.StartNew();
            do
            {
                var remoteTask = TaskQueue.CreateTask(new SlowTaskData() {TimeMs = 1000});
                remoteTask.Queue();
                taskIds.Add(remoteTask.Id);
                Thread.Sleep(100);
                var processedCountFromCounter = TestCounterRepository.GetCounter("SlowTaskHandler_Started") - TestCounterRepository.GetCounter("SlowTaskHandler_Finished");
                var processingTaskCount = Monitoring.GetProcessingTaskCount();

                Console.WriteLine("InProgress={0} Counter={1}", processedCountFromCounter, processingTaskCount.Count);
            } while(w.ElapsedMilliseconds < 10 * 1000);
            WaitForTasks(taskIds, TimeSpan.FromMinutes(15));
            WaitFor(() => Monitoring.GetProcessingTaskCount().Count == 0, TimeSpan.FromSeconds(100));
        }

        private static void WaitFor(Func<bool> func, TimeSpan timeout, int checkTimeout = 99)
        {
            var stopwatch = Stopwatch.StartNew();
            while(stopwatch.Elapsed < timeout)
            {
                Thread.Sleep(checkTimeout);
                if(func())
                    return;
            }
            Assert.Fail("Условия ожидания не выполнены за {0}", timeout);
        }

        [Test]
        public void TestRestart()
        {
            var now = DateTime.UtcNow;
            Monitoring.RestartProcessingTaskCounter(now);
            var processingTaskCount = Monitoring.GetProcessingTaskCount();
            processingTaskCount.UpdateTicks = 0;
            processingTaskCount.Counts = null;
            processingTaskCount.ShouldBeEquivalentTo(new TaskCount
                {
                    Count = 0,
                    StartTicks = now.Ticks
                });
            Thread.Sleep(100);
            Monitoring.RestartProcessingTaskCounter(null);

            processingTaskCount = Monitoring.GetProcessingTaskCount();
            var startTicks = processingTaskCount.StartTicks;
            var dateTime = now - TimeSpan.FromDays(3);
            Assert.That(startTicks >= dateTime.Ticks);
        }

        [Test, Ignore]
        public void TestCounterHardAndVerySlow()
        {
            Console.WriteLine("u=" + EventLogRepository.UnstableZoneLength);
            RunTasksAndWatiForCounterZero(false, 0, (long)TimeSpan.FromTicks(EventLogRepository.UnstableZoneLength.Ticks * 5).TotalMilliseconds, TimeSpan.FromMinutes(15));
        }

        [Test]
        public void TestCounterHardAndSlow()
        {
            WaitFor(() => Monitoring.GetProcessingTaskCount().Count == 0, TimeSpan.FromSeconds(10));
            var w = Stopwatch.StartNew();
            var taskIds = new List<string>();
            const int count = 200;
            for(var i = 0; i < count; i++)
            {
                var remoteTask = TaskQueue.CreateTask(new SlowTaskData() {TimeMs = 1000, UseCounter = false});
                taskIds.Add(remoteTask.Id);
                remoteTask.Queue();
            }
            var addTime = w.ElapsedMilliseconds;
            WaitForTasks(taskIds, TimeSpan.FromMinutes(1));
            var totalTime = w.ElapsedMilliseconds;
            var addRate = 1000.0 * count / addTime; //tasks / s
            var consumeRate = 1000.0 * count / totalTime; //NOTE consumeRate занижен тк задачи добавляются последовательно
            Console.WriteLine("{0:F0} : {1:F0}", addRate, consumeRate);
            if(addRate < consumeRate * 2)
                Console.WriteLine("WARN: Slow");
            //Assert.That(addRate > consumeRate * 2);
            var delayMs = (int)((1 / consumeRate - 1 / addRate) * 1000) / 2;
            if(delayMs < 0)
                delayMs = 0;
            Console.WriteLine("Calculated delay {0} ms", delayMs);

            var testTime = TimeSpan.FromTicks(EventLogRepository.UnstableZoneLength.Ticks * 5);
            Console.WriteLine("test={0:F1} min", testTime.TotalMinutes);
            var estimatedTaskRunTime = TimeSpan.FromMilliseconds(testTime.TotalMilliseconds * addRate / consumeRate);
            Console.WriteLine("est={0:F1} min", estimatedTaskRunTime.TotalMinutes);
            RunTasksAndWatiForCounterZero(false, delayMs, (long)testTime.TotalMilliseconds, TimeSpan.FromMinutes(15));
        }

        private void RunTasksAndWatiForCounterZero(bool useTaskCounter, int addDelay, long addTime, TimeSpan waitTasksTime)
        {
            var taskIds = new List<string>();
            var w = Stopwatch.StartNew();
            do
            {
                var remoteTask = TaskQueue.CreateTask(new SlowTaskData {TimeMs = 1000, UseCounter = useTaskCounter});
                taskIds.Add(remoteTask.Id);
                remoteTask.Queue();
                if(addDelay > 0)
                    Thread.Sleep(addDelay);
            } while(w.ElapsedMilliseconds < addTime);
            Console.WriteLine("Waiting for all tasks finished");
            //WaitFor(() => taskQueue.GetTaskInfo<SlowTaskData>(lastTaskId).Context.State == TaskState.Finished, TimeSpan.FromSeconds(3));
            WaitForTasks(taskIds, waitTasksTime);
            Console.WriteLine("Waiting for Counter");
            WaitFor(() => Monitoring.GetProcessingTaskCount().Count == 0, waitTasksTime);
        }

        private void WaitForTasks(List<string> taskIds, TimeSpan timeSpan)
        {
            WaitFor(() =>
                {
                    foreach(var taskId in taskIds)
                    {
                        if(TaskQueue.GetTaskInfo<SlowTaskData>(taskId).Context.State != TaskState.Finished)
                            return false;
                    }
                    return true;
                }, timeSpan);
        }

        [Injected]
        private readonly ITestCounterRepository TestCounterRepository;

        [Injected]
        private readonly IRemoteTaskQueue TaskQueue;

        [Injected]
        private readonly IRemoteTaskQueueTaskCounterClient Monitoring;

        [Injected]
        private readonly IEventLogRepository EventLogRepository;

        [Injected]
        private readonly IContainer container;
    }
}