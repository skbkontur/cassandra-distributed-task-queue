using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

using RemoteTaskQueue.FunctionalTests.Common.ConsumerStateImpl;
using RemoteTaskQueue.FunctionalTests.Common.TaskDatas;

using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.ServiceLib.Logging;

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
            Log.For(this).InfoFormat("Start test. LoggingId = {0}", loggingId);
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
            Log.For(this).InfoFormat("Found {0} chains, as expected", chains.Length);
            foreach (var grouping in chains)
                CheckChain(grouping.Key, grouping.ToArray());
            Log.For(this).Info("Checking chains success");
        }

        private void CheckChain(string chainName, RemoteTaskInfo<ChainTaskData>[] chain)
        {
            Log.For(this).InfoFormat("Start check chain '{0}'", chainName);
            Assert.That(chain.Length, Is.EqualTo(tasksInChain), string.Format("Количество задач в цепочке должно быть равно {0}", tasksInChain));
            var ordered = chain.OrderBy(info => info.TaskData.ChainPosition).ToArray();
            string previousTaskId = null;
            foreach (var taskInfo in ordered)
            {
                Assert.That(taskInfo.Context.ParentTaskId, Is.EqualTo(previousTaskId), string.Format("Не выполнилось ожидание правильного ParentTaskId для таски {0}", taskInfo.Context.Id));
                previousTaskId = taskInfo.Context.Id;
            }
            Log.For(this).InfoFormat("Check chain '{0}' success", chainName);
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
                    Log.For(this).InfoFormat("Read {0} tasks, expected {1} tasks. Sleep", ids.Length, expectedTasks);
                    Thread.Sleep(sleepInterval);
                    continue;
                }
                if (ids.Length > expectedTasks)
                    throw new Exception(string.Format("Found {0} tasks, when expected {1} tasks", ids.Length, expectedTasks));
                Log.For(this).InfoFormat("Found {0} tasks, as expected", ids.Length);
                var taskInfos = remoteTaskQueue.GetTaskInfos<ChainTaskData>(ids);
                var finished = taskInfos.Where(info => info.Context.State == TaskState.Finished).ToArray();
                var notFinished = taskInfos.Where(info => info.Context.State != TaskState.Finished).ToArray();
                if (notFinished.Length != 0)
                {
                    Log.For(this).InfoFormat("Found {0} finished tasks, but {1} not finished. Sleep", finished.Length, notFinished.Length);
                    Thread.Sleep(sleepInterval);
                    continue;
                }
                Log.For(this).InfoFormat("Found {0} finished tasks, as expected. Finish waiting", finished.Length);
                return taskInfos;
            }
        }

        [Injected]
        private readonly ITestTaskLogger testTaskLogger;
    }
}