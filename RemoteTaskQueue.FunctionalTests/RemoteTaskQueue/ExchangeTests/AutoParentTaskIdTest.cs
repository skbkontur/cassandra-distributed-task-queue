using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using GroboContainer.NUnitExtensions;

using NUnit.Framework;

using RemoteTaskQueue.FunctionalTests.Common.ConsumerStateImpl;
using RemoteTaskQueue.FunctionalTests.Common.TaskDatas;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

using SKBKontur.Catalogue.ServiceLib.Logging;

using Vostok.Logging.Abstractions;

namespace RemoteTaskQueue.FunctionalTests.RemoteTaskQueue.ExchangeTests
{
    public class AutoParentTaskIdTest : ExchangeTestBase
    {
        [TestCase(1, TestName = "TestOneChain")]
        [TestCase(10, TestName = "TestTenChains")]
        [TestCase(100, TestName = "TestManyChains")]
        public void Test(int chainsCount)
        {
            var loggingId = Guid.NewGuid().ToString();
            Log.For(this).Info($"Start test. LoggingId = {loggingId}");
            for (var i = 0; i < chainsCount; i++)
            {
                remoteTaskQueue.CreateTask(new ChainTaskData
                    {
                        LoggingTaskIdKey = loggingId,
                        ChainName = string.Format("Chain {0}", i),
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
            Assert.That(chains.Length, Is.EqualTo(infos.Length / tasksInChain),
                        string.Format("Количество цепочек должно быть равно общему числу тасков, деленному на {0}", tasksInChain));
            Log.For(this).Info($"Found {chains.Length} chains, as expected");
            foreach (var grouping in chains)
                CheckChain(grouping.Key, grouping.ToArray());
            Log.For(this).Info("Checking chains success");
        }

        private void CheckChain(string chainName, RemoteTaskInfo<ChainTaskData>[] chain)
        {
            Log.For(this).Info($"Start check chain '{chainName}'");
            Assert.That(chain.Length, Is.EqualTo(tasksInChain), string.Format("Количество задач в цепочке должно быть равно {0}", tasksInChain));
            var ordered = chain.OrderBy(info => info.TaskData.ChainPosition).ToArray();
            string previousTaskId = null;
            foreach (var taskInfo in ordered)
            {
                Assert.That(taskInfo.Context.ParentTaskId, Is.EqualTo(previousTaskId), string.Format("Не выполнилось ожидание правильного ParentTaskId для таски {0}", taskInfo.Context.Id));
                previousTaskId = taskInfo.Context.Id;
            }
            Log.For(this).Info($"Check chain '{chainName}' success");
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
                    Log.For(this).Info($"Read {ids.Length} tasks, expected {expectedTasks} tasks. Sleep");
                    Thread.Sleep(sleepInterval);
                    continue;
                }
                if (ids.Length > expectedTasks)
                    throw new Exception(string.Format("Found {0} tasks, when expected {1} tasks", ids.Length, expectedTasks));
                Log.For(this).Info($"Found {ids.Length} tasks, as expected");
                var taskInfos = remoteTaskQueue.GetTaskInfos<ChainTaskData>(ids);
                var finished = taskInfos.Where(info => info.Context.State == TaskState.Finished).ToArray();
                var notFinished = taskInfos.Where(info => info.Context.State != TaskState.Finished).ToArray();
                if (notFinished.Length != 0)
                {
                    Log.For(this).Info($"Found {finished.Length} finished tasks, but {notFinished.Length} not finished. Sleep");
                    Thread.Sleep(sleepInterval);
                    continue;
                }
                Log.For(this).Info($"Found {finished.Length} finished tasks, as expected. Finish waiting");
                return taskInfos;
            }
        }

        [Injected]
        private readonly ITestTaskLogger testTaskLogger;
    }
}