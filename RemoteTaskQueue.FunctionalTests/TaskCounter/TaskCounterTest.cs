using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using ExchangeService.UserClasses;

using FluentAssertions;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Handling;

using RemoteTaskQueue.FunctionalTests.Common;
using RemoteTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData;
using RemoteTaskQueue.TaskCounter;

using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.ServiceLib.Logging;

namespace RemoteTaskQueue.FunctionalTests.TaskCounter
{
    [EdiTestSuite("TaskCounterTestSuite"), WithTestRemoteTaskQueue, AndResetExchangeServiceState]
    public class TaskCounterTest
    {
        [EdiSetUp]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public void SetUp()
        {
            taskCounterServiceClient.RestartProcessingTaskCounter(DateTime.UtcNow);
        }

        [Test, Ignore]
        public void TestOldWaitingTaskCount()
        {
            //note long test
            exchangeServiceClient.Stop();
            taskQueue.CreateTask(new SlowTaskData {TimeMs = 1}).Queue();
            taskQueue.CreateTask(new SlowTaskData {TimeMs = 1}).Queue();
            Thread.Sleep(TimeSpan.FromMinutes(6));
            TaskCount count = null;
            WaitFor(() => (count = taskCounterServiceClient.GetProcessingTaskCount()).OldWaitingTaskCount == 2, TimeSpan.FromSeconds(10));
            Assert.IsNotNull(count);

            exchangeServiceClient.Start();
            WaitFor(() =>
                {
                    var taskCount = taskCounterServiceClient.GetProcessingTaskCount();
                    return taskCount.Count == 0 && taskCount.OldWaitingTaskCount == 0;
                }, TimeSpan.FromSeconds(10));
        }

        [Test]
        public void TestCounts()
        {
            exchangeServiceClient.Stop();
            taskQueue.CreateTask(new SlowTaskData {TimeMs = 1}).Queue();
            taskQueue.CreateTask(new SlowTaskData {TimeMs = 1}).Queue();
            TaskCount count = null;
            WaitFor(() => (count = taskCounterServiceClient.GetProcessingTaskCount()).Count == 2, TimeSpan.FromSeconds(10));
            Assert.IsNotNull(count);
            Assert.AreEqual(2, count.Counts[(int)TaskState.New]);

            exchangeServiceClient.Start();
            WaitFor(() => taskCounterServiceClient.GetProcessingTaskCount().Count == 0, TimeSpan.FromSeconds(10));
        }

        [Test]
        public void TestCounter()
        {
            WaitFor(() => taskCounterServiceClient.GetProcessingTaskCount().Count == 0, TimeSpan.FromSeconds(10));
            var taskIds = new List<string>();
            var w = Stopwatch.StartNew();
            do
            {
                var remoteTask = taskQueue.CreateTask(new SlowTaskData {TimeMs = 1000});
                remoteTask.Queue();
                taskIds.Add(remoteTask.Id);
                Thread.Sleep(100);
                var processedCountFromCounter = testCounterRepository.GetCounter("SlowTaskHandler_Started") - testCounterRepository.GetCounter("SlowTaskHandler_Finished");
                var processingTaskCount = taskCounterServiceClient.GetProcessingTaskCount();

                Log.For(this).InfoFormat("InProgress={0} Counter={1}", processedCountFromCounter, processingTaskCount.Count);
            } while(w.ElapsedMilliseconds < 10 * 1000);
            WaitForTasks(taskIds, TimeSpan.FromMinutes(15));
            WaitFor(() => taskCounterServiceClient.GetProcessingTaskCount().Count == 0, TimeSpan.FromSeconds(100));
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
            taskCounterServiceClient.RestartProcessingTaskCounter(now);
            var processingTaskCount = taskCounterServiceClient.GetProcessingTaskCount();
            processingTaskCount.UpdateTicks = 0;
            processingTaskCount.Counts = null;
            processingTaskCount.ShouldBeEquivalentTo(new TaskCount
                {
                    Count = 0,
                    StartTicks = now.Ticks
                });
            Thread.Sleep(100);
            taskCounterServiceClient.RestartProcessingTaskCounter(null);

            processingTaskCount = taskCounterServiceClient.GetProcessingTaskCount();
            var startTicks = processingTaskCount.StartTicks;
            var dateTime = now - TimeSpan.FromDays(3);
            Assert.That(startTicks >= dateTime.Ticks);
        }

        [Test]
        public void TestCounterHardAndSlow()
        {
            WaitFor(() => taskCounterServiceClient.GetProcessingTaskCount().Count == 0, TimeSpan.FromSeconds(10));
            var w = Stopwatch.StartNew();
            var taskIds = new List<string>();
            const int count = 200;
            for(var i = 0; i < count; i++)
            {
                var remoteTask = taskQueue.CreateTask(new SlowTaskData {TimeMs = 1000, UseCounter = false});
                taskIds.Add(remoteTask.Id);
                remoteTask.Queue();
            }
            var addTime = w.ElapsedMilliseconds;
            WaitForTasks(taskIds, TimeSpan.FromMinutes(1));
            var totalTime = w.ElapsedMilliseconds;
            var addRate = 1000.0 * count / addTime; //tasks / s
            var consumeRate = 1000.0 * count / totalTime; //NOTE consumeRate занижен тк задачи добавляются последовательно
            Log.For(this).InfoFormat("{0:F0} : {1:F0}", addRate, consumeRate);
            if(addRate < consumeRate * 2)
                Log.For(this).Warn("WARN: Slow");
            //Assert.That(addRate > consumeRate * 2);
            var delayMs = (int)((1 / consumeRate - 1 / addRate) * 1000) / 2;
            if(delayMs < 0)
                delayMs = 0;
            Log.For(this).InfoFormat("Calculated delay {0} ms", delayMs);

            var testTime = TimeSpan.FromTicks(eventLogRepository.UnstableZoneLength.Ticks * 5);
            Log.For(this).InfoFormat("test={0:F1} min", testTime.TotalMinutes);
            var estimatedTaskRunTime = TimeSpan.FromMilliseconds(testTime.TotalMilliseconds * addRate / consumeRate);
            Log.For(this).InfoFormat("est={0:F1} min", estimatedTaskRunTime.TotalMinutes);
            RunTasksAndWatiForCounterZero(false, delayMs, (long)testTime.TotalMilliseconds, TimeSpan.FromMinutes(15));
        }

        private void RunTasksAndWatiForCounterZero(bool useTaskCounter, int addDelay, long addTime, TimeSpan waitTasksTime)
        {
            var taskIds = new List<string>();
            var w = Stopwatch.StartNew();
            do
            {
                var remoteTask = taskQueue.CreateTask(new SlowTaskData {TimeMs = 1000, UseCounter = useTaskCounter});
                taskIds.Add(remoteTask.Id);
                remoteTask.Queue();
                if(addDelay > 0)
                    Thread.Sleep(addDelay);
            } while(w.ElapsedMilliseconds < addTime);
            Log.For(this).Info("Waiting for all tasks finished");
            //WaitFor(() => taskQueue.GetTaskInfo<SlowTaskData>(lastTaskId).Context.State == TaskState.Finished, TimeSpan.FromSeconds(3));
            WaitForTasks(taskIds, waitTasksTime);
            Log.For(this).Info("Waiting for Counter");
            WaitFor(() => taskCounterServiceClient.GetProcessingTaskCount().Count == 0, waitTasksTime);
        }

        private void WaitForTasks(List<string> taskIds, TimeSpan timeSpan)
        {
            WaitFor(() =>
                {
                    foreach(var taskId in taskIds)
                    {
                        if(taskQueue.GetTaskInfo<SlowTaskData>(taskId).Context.State != TaskState.Finished)
                            return false;
                    }
                    return true;
                }, timeSpan);
        }

        [Injected]
        private readonly ITestCounterRepository testCounterRepository;

        [Injected]
        private readonly IRemoteTaskQueue taskQueue;

        [Injected]
        private readonly TaskCounterServiceClient taskCounterServiceClient;

        [Injected]
        private readonly IEventLogRepository eventLogRepository;

        [Injected]
        private readonly ExchangeServiceClient exchangeServiceClient;
    }
}