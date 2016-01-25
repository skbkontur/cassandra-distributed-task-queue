using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using ExchangeService.Exceptions;
using ExchangeService.UserClasses;

using MoreLinq;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;

using SKBKontur.Catalogue.ServiceLib.Logging;

namespace FunctionalTests.ExchangeTests
{
    public abstract class TasksWithCounterTestBase : FunctionalTestBase
    {
        protected void Wait(string[] taskIds, TaskState terminalState, string taskName, TimeSpan timeout)
        {
            var sw = Stopwatch.StartNew();
            var sleepInterval = Math.Max(500, (int)timeout.TotalMilliseconds / 10);
            while(true)
            {
                var allTasksAreFinished = handleTaskCollection.GetTasks(taskIds).All(x => x.Meta.State == terminalState);
                var attempts = taskIds.Select(testCounterRepository.GetCounter).ToArray();
                Log.For(this).InfoFormat("CurrentCounterValues: {0}", string.Join(", ", attempts));
                var notFinishedTaskIds = taskIds.EquiZip(attempts, (taskId, attempt) => new {taskId, attempt}).Where(x => x.attempt > 0).Select(x => x.taskId).ToArray();
                if(allTasksAreFinished)
                {
                    Assert.That(notFinishedTaskIds, Is.Empty);
                    try
                    {
                        Container.CheckTaskMinimalStartTicksIndexStates(taskIds.ToDictionary(s => s, s => TaskIndexShardKey(taskName, terminalState)));
                        break;
                    }
                    catch(AssertionException e)
                    {
                        if(sw.Elapsed > timeout)
                            throw;
                        Log.For(this).Warn("Maybe we have a stability issue because index records are being removed after new task meta has been written. Will keep waiting.", e);
                    }
                }
                if(sw.Elapsed > timeout)
                    throw new TooLateException("Время ожидания превысило {0} мс. NotFinihedTaskIds: {1}", timeout, string.Join(", ", notFinishedTaskIds));
                Thread.Sleep(sleepInterval);
            }
        }

        protected IHandleTaskCollection handleTaskCollection;
        protected ITestCounterRepository testCounterRepository;
    }
}