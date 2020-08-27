using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using GroboContainer.NUnitExtensions;

using NUnit.Framework;

using RemoteTaskQueue.FunctionalTests.Common.ConsumerStateImpl;
using RemoteTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories;

using SKBKontur.Catalogue.ServiceLib.Logging;
using SKBKontur.Catalogue.Waiting;

using Vostok.Logging.Abstractions;
#if !NETCOREAPP
using Vostok.Commons.Time;

#endif

namespace RemoteTaskQueue.FunctionalTests.Monitoring
{
    public class TaskCounterTest : MonitoringTestBase
    {
        [Test]
        public void LostTasksCount()
        {
            exchangeServiceClient.Stop();
            remoteTaskQueue.CreateTask(new SlowTaskData {TimeMs = 1}).Queue();
            remoteTaskQueue.CreateTask(new SlowTaskData {TimeMs = 1}).Queue();
            Thread.Sleep(TimeSpan.FromSeconds(10));
            WaitFor(() => monitoringServiceClient.GetTaskCounters().LostTasksCount == 2, TimeSpan.FromSeconds(10));

            exchangeServiceClient.Start();
            WaitFor(() =>
                {
                    var taskCount = monitoringServiceClient.GetTaskCounters();
                    return taskCount.GetPendingTaskTotalCount() == 0 && taskCount.LostTasksCount == 0;
                }, TimeSpan.FromSeconds(10));
        }

        [Test]
        public void TestCounts()
        {
            exchangeServiceClient.Stop();
            remoteTaskQueue.CreateTask(new SlowTaskData {TimeMs = 1}).Queue();
            remoteTaskQueue.CreateTask(new SlowTaskData {TimeMs = 1}).Queue();
            WaitFor(() => monitoringServiceClient.GetTaskCounters().PendingTaskCountsTotal[TaskState.New] == 2, TimeSpan.FromSeconds(10));
            exchangeServiceClient.Start();
            WaitFor(() => monitoringServiceClient.GetTaskCounters().GetPendingTaskTotalCount() == 0, TimeSpan.FromSeconds(10));
        }

        [Test]
        public void TestCounter()
        {
            WaitFor(() => monitoringServiceClient.GetTaskCounters().GetPendingTaskTotalCount() == 0, TimeSpan.FromSeconds(10));
            var taskIds = new List<string>();
            var w = Stopwatch.StartNew();
            do
            {
                var remoteTask = remoteTaskQueue.CreateTask(new SlowTaskData {TimeMs = 1000});
                remoteTask.Queue();
                taskIds.Add(remoteTask.Id);
                Thread.Sleep(100);
                var processedCountFromCounter = testCounterRepository.GetCounter("SlowTaskHandler_Started") - testCounterRepository.GetCounter("SlowTaskHandler_Finished");
                var processingTaskCount = monitoringServiceClient.GetTaskCounters();
                Log.For(this).Info("InProgress={InProgressCount} Counter={Counter}", new {InProgressCount = processedCountFromCounter, Counter = processingTaskCount.GetPendingTaskTotalCount()});
            } while (w.Elapsed < TimeSpan.FromSeconds(10));
            WaitForTasks(taskIds, TimeSpan.FromMinutes(1));
            WaitFor(() => monitoringServiceClient.GetTaskCounters().GetPendingTaskTotalCount() == 0, TimeSpan.FromSeconds(10));
        }

        [Test]
        public void TestCounterHardAndSlow()
        {
            WaitFor(() => monitoringServiceClient.GetTaskCounters().GetPendingTaskTotalCount() == 0, TimeSpan.FromSeconds(10));
            var w = Stopwatch.StartNew();
            var taskIds = new List<string>();
            const int count = 200;
            for (var i = 0; i < count; i++)
            {
                var remoteTask = remoteTaskQueue.CreateTask(new SlowTaskData {TimeMs = 1000, UseCounter = false});
                taskIds.Add(remoteTask.Id);
                remoteTask.Queue();
            }
            var addTime = w.ElapsedMilliseconds;
            WaitForTasks(taskIds, TimeSpan.FromMinutes(1));
            var totalTime = w.ElapsedMilliseconds;
            var addRate = 1000.0 * count / addTime; //tasks / s
            var consumeRate = 1000.0 * count / totalTime; //NOTE consumeRate занижен тк задачи добавляются последовательно
            Log.For(this).Info("{AddRate} : {ConsumeRate}", new {AddRate = addRate.ToString("F0"), ConsumeRate = consumeRate.ToString("F0")});
            if (addRate < consumeRate * 2)
                Log.For(this).Warn("WARN: Slow");
            //Assert.That(addRate > consumeRate * 2);
            var delayMs = (int)((1 / consumeRate - 1 / addRate) * 1000) / 2;
            if (delayMs < 0)
                delayMs = 0;
            Log.For(this).Info("Calculated delay {DelayMs} ms", new {DelayMs = delayMs});

            var testTime = eventLogRepository.GetUnstableZoneDuration().Multiply(5);
            Log.For(this).Info("test={TestTimeInMinutes} min", new {TestTimeInMinutes = testTime.TotalMinutes.ToString("F1")});
            var estimatedTaskRunTime = TimeSpan.FromMilliseconds(testTime.TotalMilliseconds * addRate / consumeRate);
            Log.For(this).Info("est={EstimatedTaskRunTimeInMinutes} min", new {EstimatedTaskRunTimeInMinutes = estimatedTaskRunTime.TotalMinutes.ToString("F1")});
            RunTasksAndWaitForCounterZero(false, delayMs, (long)testTime.TotalMilliseconds, TimeSpan.FromMinutes(15));
        }

        private void RunTasksAndWaitForCounterZero(bool useTaskCounter, int addDelay, long addTime, TimeSpan waitTasksTime)
        {
            var taskIds = new List<string>();
            var w = Stopwatch.StartNew();
            do
            {
                var remoteTask = remoteTaskQueue.CreateTask(new SlowTaskData {TimeMs = 1000, UseCounter = useTaskCounter});
                taskIds.Add(remoteTask.Id);
                remoteTask.Queue();
                if (addDelay > 0)
                    Thread.Sleep(addDelay);
            } while (w.ElapsedMilliseconds < addTime);
            Log.For(this).Info("Waiting for all tasks finished");
            WaitForTasks(taskIds, waitTasksTime);
            Log.For(this).Info("Waiting for Counter");
            WaitFor(() => monitoringServiceClient.GetTaskCounters().GetPendingTaskTotalCount() == 0, waitTasksTime);
        }

        private void WaitForTasks(List<string> taskIds, TimeSpan timeout)
        {
            WaitFor(() => taskIds.All(taskId => remoteTaskQueue.GetTaskInfo<SlowTaskData>(taskId).Context.State == TaskState.Finished), timeout);
        }

        private static void WaitFor(Func<bool> func, TimeSpan timeout)
        {
            WaitHelper.Wait(() => func() ? WaitResult.StopWaiting : WaitResult.ContinueWaiting, timeout);
        }

        [Injected]
        private readonly ITestCounterRepository testCounterRepository;

        [Injected]
        private readonly EventLogRepository eventLogRepository;

        [Injected]
        private readonly ExchangeServiceClient exchangeServiceClient;
    }
}