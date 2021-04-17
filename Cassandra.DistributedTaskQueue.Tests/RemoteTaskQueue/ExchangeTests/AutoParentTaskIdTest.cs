using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using GroboContainer.NUnitExtensions;

using NUnit.Framework;

using RemoteTaskQueue.FunctionalTests.Common;
using RemoteTaskQueue.FunctionalTests.Common.ConsumerStateImpl;
using RemoteTaskQueue.FunctionalTests.Common.TaskDatas;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests.RemoteTaskQueue.ExchangeTests
{
    public class AutoParentTaskIdTest : ExchangeTestBase
    {
        [TestCase(1, TestName = "TestOneChain")]
        [TestCase(10, TestName = "TestTenChains")]
        [TestCase(100, TestName = "TestManyChains")]
        public void Test(int chainsCount)
        {
            var loggingId = Guid.NewGuid().ToString();
            Log.For(this).Info("Start test. LoggingId = {LoggingId}", new {LoggingId = loggingId});
            for (var i = 0; i < chainsCount; i++)
            {
                remoteTaskQueue.CreateTask(new ChainTaskData
                    {
                        LoggingTaskIdKey = loggingId,
                        ChainName = $"Chain {i}",
                        ChainPosition = 0
                    }).Queue();
            }
            var taskInfos = WaitLoggedTasks(loggingId, chainsCount * tasksInChain, TimeSpan.FromMinutes(2));
            CheckChains(taskInfos);
        }

        private const int tasksInChain = 10;

        private void CheckChains(RemoteTaskInfo<ChainTaskData>[] infos)
        {
            Log.For(this).Info("Start checking chains");
            var chains = infos.GroupBy(info => info.TaskData.ChainName).ToArray();
            Assert.That(chains.Length,
                        Is.EqualTo(infos.Length / tasksInChain),
                        $"Количество цепочек должно быть равно общему числу тасков, деленному на {tasksInChain}");
            Log.For(this).Info("Found {ChainsCount} chains, as expected", new {ChainsCount = chains.Length});
            foreach (var grouping in chains)
                CheckChain(grouping.Key, grouping.ToArray());
            Log.For(this).Info("Checking chains success");
        }

        private void CheckChain(string chainName, RemoteTaskInfo<ChainTaskData>[] chain)
        {
            Log.For(this).Info("Start check chain '{ChainName}'", new {ChainName = chainName});
            Assert.That(chain.Length, Is.EqualTo(tasksInChain), $"Количество задач в цепочке должно быть равно {tasksInChain}");
            var ordered = chain.OrderBy(info => info.TaskData.ChainPosition).ToArray();
            string previousTaskId = null;
            foreach (var taskInfo in ordered)
            {
                Assert.That(taskInfo.Context.ParentTaskId, Is.EqualTo(previousTaskId), $"Не выполнилось ожидание правильного ParentTaskId для таски {taskInfo.Context.Id}");
                previousTaskId = taskInfo.Context.Id;
            }
            Log.For(this).Info("Check chain '{ChainName}' success", new {ChainName = chainName});
        }

        private RemoteTaskInfo<ChainTaskData>[] WaitLoggedTasks(string loggingId, int expectedTasks, TimeSpan timeout)
        {
            const int sleepInterval = 5000;
            var stopwatch = Stopwatch.StartNew();
            while (true)
            {
                if (stopwatch.Elapsed > timeout)
                    throw new TooLateException("Время ожидания превысило {0} мс.", timeout);
                var ids = testTaskLogger.GetAll(loggingId);
                if (ids.Length < expectedTasks)
                {
                    Log.For(this).Info("Read {ReadTasksCount} tasks, expected {ExpectedTasksCount} tasks. Sleep", new {ReadTasksCount = ids.Length, ExpectedTasksCount = expectedTasks});
                    Thread.Sleep(sleepInterval);
                    continue;
                }
                if (ids.Length > expectedTasks)
                    throw new Exception($"Found {ids.Length} tasks, when expected {expectedTasks} tasks");
                Log.For(this).Info("Found {ReadTasksCount} tasks, as expected", new {ReadTasksCount = ids.Length});
                var taskInfos = remoteTaskQueue.GetTaskInfos<ChainTaskData>(ids);
                var finished = taskInfos.Where(info => info.Context.State == TaskState.Finished).ToArray();
                var notFinished = taskInfos.Where(info => info.Context.State != TaskState.Finished).ToArray();
                if (notFinished.Length != 0)
                {
                    Log.For(this).Info("Found {FinishedCount} finished tasks, but {NotFinishedCount} not finished. Sleep",
                                       new {FinishedCount = finished.Length, NotFinishedCount = notFinished.Length});
                    Thread.Sleep(sleepInterval);
                    continue;
                }
                Log.For(this).Info("Found {FinishedCount} finished tasks, as expected. Finish waiting", new {FinishedCount = finished.Length});
                return taskInfos;
            }
        }

        [Injected]
        private readonly ITestTaskLogger testTaskLogger;
    }
}