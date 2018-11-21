using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

using GroboContainer.NUnitExtensions;

using MoreLinq;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;

using RemoteTaskQueue.FunctionalTests.Common.ConsumerStateImpl;

using SKBKontur.Catalogue.ServiceLib.Logging;

namespace RemoteTaskQueue.FunctionalTests.RemoteTaskQueue.ExchangeTests
{
    public abstract class TasksWithCounterTestBase : ExchangeTestBase
    {
        protected void WaitForTerminalState(string[] taskIds, TaskState terminalState, string taskName, TimeSpan timeout, TimeSpan? sleepInterval = null)
        {
            var sw = Stopwatch.StartNew();
            sleepInterval = sleepInterval ?? TimeSpan.FromMilliseconds(Math.Max(500, (int)timeout.TotalMilliseconds / 10));
            while (true)
            {
                var allTasksAreFinished = handleTaskCollection.GetTasks(taskIds).All(x => x.Meta.State == terminalState);
                var attempts = taskIds.Select(testCounterRepository.GetCounter).ToArray();
                Log.For(this).InfoFormat("CurrentCounterValues: {0}", string.Join(", ", attempts));
                var notFinishedTaskIds = taskIds.EquiZip(attempts, (taskId, attempt) => new {taskId, attempt}).Where(x => x.attempt > 0).Select(x => x.taskId).ToArray();
                if (allTasksAreFinished)
                {
                    Assert.That(notFinishedTaskIds, Is.Empty);
                    try
                    {
                        CheckTaskMinimalStartTicksIndexStates(taskIds.ToDictionary(s => s, s => TaskIndexShardKey(taskName, terminalState)));
                        break;
                    }
                    catch (AssertionException e)
                    {
                        if (sw.Elapsed > timeout)
                            throw;
                        Log.For(this).Warn("Maybe we have a stability issue because index records are being removed after new task meta has been written. Will keep waiting.", e);
                    }
                }
                if (sw.Elapsed > timeout)
                    throw new TooLateException("Время ожидания превысило {0} мс. NotFinihedTaskIds: {1}", timeout, string.Join(", ", notFinishedTaskIds));
                Thread.Sleep(sleepInterval.Value);
            }
        }

        protected void WaitForState(string[] taskIds, TaskState targetState, TimeSpan timeout, TimeSpan? sleepInterval = null)
        {
            var sw = Stopwatch.StartNew();
            sleepInterval = sleepInterval ?? TimeSpan.FromMilliseconds(Math.Max(500, (int)timeout.TotalMilliseconds / 10));
            while (true)
            {
                if (handleTaskCollection.GetTasks(taskIds).All(x => x.Meta.State == targetState))
                    break;
                if (sw.Elapsed > timeout)
                    throw new TooLateException("Время ожидания превысило {0} мс. Tasks in another state: {1}", timeout,
                                               string.Join(", ", handleTaskCollection.GetTasks(taskIds).Where(x => x.Meta.State != targetState).Select(x => x.Meta.Id)));
                Thread.Sleep(sleepInterval.Value);
            }
        }

        [Injected]
        private readonly IHandleTaskCollection handleTaskCollection;

        [Injected]
        [SuppressMessage("ReSharper", "UnassignedReadonlyField")]
        protected readonly ITestCounterRepository testCounterRepository;
    }
}