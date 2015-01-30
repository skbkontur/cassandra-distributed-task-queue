using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using ExchangeService.UserClasses;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;

namespace FunctionalTests.ExchangeTests
{
    public class TaskCounterTest : FunctionalTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            //todo
            monitoring = Container.Get<IRemoteTaskQueueMonitoringServiceStorage>();
            testCounterRepository = Container.Get<ITestCounterRepository>();
            taskQueue = Container.Get<IRemoteTaskQueue>();
            eventLogRepository = Container.Get<IEventLogRepository>();
        }

        [Test]
        public void TestCounter()
        {
            var taskIds = new List<string>();
            var w = Stopwatch.StartNew();
            do
            {
                var remoteTask = taskQueue.CreateTask<SlowTaskData>(new SlowTaskData() {TimeMs = 1000});
                remoteTask.Queue();
                taskIds.Add(remoteTask.Id);
                Thread.Sleep(100);
                var processedCountFromCounter = testCounterRepository.GetCounter("SlowTaskHandler_Started") - testCounterRepository.GetCounter("SlowTaskHandler_Finished");
                var processingTaskCount = monitoring.GetProcessingTaskCount();

                Console.WriteLine("InProgress={0} Counter={1}", processedCountFromCounter, processingTaskCount.Count);
            } while(w.ElapsedMilliseconds < 10 * 1000);
            //} while (w.ElapsedMilliseconds < TimeSpan.FromTicks(eventLogRepository.UnstableZoneLength.Ticks * 5).TotalMilliseconds);
            WaitForTasks(taskIds, TimeSpan.FromMinutes(15));
            WaitFor(() => monitoring.GetProcessingTaskCount().Count == 0, TimeSpan.FromSeconds(10));
        }

        [Test]
        public void TestRestart()
        {
            var now = DateTime.UtcNow;
            monitoring.RestartProcessgingTaskCounter(now);
            var processingTaskCount = monitoring.GetProcessingTaskCount();
            processingTaskCount.UpdateTicks = 0;
            processingTaskCount.AssertEqualsTo(new TaskCount()
                {
                    Count = 0,
                    StartTicks = now.Ticks
                });
            Thread.Sleep(100);
            monitoring.RestartProcessgingTaskCounter(null);

            processingTaskCount = monitoring.GetProcessingTaskCount();
            var startTicks = processingTaskCount.StartTicks;
            var dateTime = now - TimeSpan.FromDays(3);
            Assert.That(startTicks >= dateTime.Ticks);
        }

        [Test, Ignore]
        public void TestCounterHardAndVerySlow()
        {
            Console.WriteLine("u=" + eventLogRepository.UnstableZoneLength);
            RunTasksAndWatiForCounterZero(false, 0, (long)TimeSpan.FromTicks(eventLogRepository.UnstableZoneLength.Ticks * 5).TotalMilliseconds, TimeSpan.FromMinutes(15));
        }

        [Test]
        public void TestCounterHardAndSlow()
        {
            var w = Stopwatch.StartNew();
            var taskIds = new List<string>();
            const int count = 200;
            for(var i = 0; i < count; i++)
            {
                var remoteTask = taskQueue.CreateTask<SlowTaskData>(new SlowTaskData() {TimeMs = 1000, UseCounter = false});
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
            {
                Console.WriteLine("WARN: Slow");
            }
            //Assert.That(addRate > consumeRate * 2);
            var delayMs = (int)((1 / consumeRate - 1 / addRate) * 1000) / 2;
            if(delayMs < 0)
                delayMs = 0;
            Console.WriteLine("Calculated delay {0} ms", delayMs);

            var testTime = TimeSpan.FromTicks(eventLogRepository.UnstableZoneLength.Ticks * 5);
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
                var remoteTask = taskQueue.CreateTask(new SlowTaskData() {TimeMs = 1000, UseCounter = useTaskCounter});
                taskIds.Add(remoteTask.Id);
                remoteTask.Queue();
                if(addDelay > 0)
                    Thread.Sleep(addDelay);
            } while(w.ElapsedMilliseconds < addTime);
            Console.WriteLine("Waiting for all tasks finished");
            //WaitFor(() => taskQueue.GetTaskInfo<SlowTaskData>(lastTaskId).Context.State == TaskState.Finished, TimeSpan.FromSeconds(3));
            WaitForTasks(taskIds, waitTasksTime);
            Console.WriteLine("Waiting for Counter");
            WaitFor(() => monitoring.GetProcessingTaskCount().Count == 0, TimeSpan.FromMinutes(1));
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

        private ITestCounterRepository testCounterRepository;
        private IRemoteTaskQueue taskQueue;
        private IRemoteTaskQueueMonitoringServiceStorage monitoring;
        private IEventLogRepository eventLogRepository;
    }
}