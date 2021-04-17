using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.Scheduling
{
    public class ThreadBasedPeriodicTaskRunner : IPeriodicTaskRunner
    {
        public ThreadBasedPeriodicTaskRunner(ILog logger)
        {
            this.logger = logger.ForContext(nameof(ThreadBasedPeriodicTaskRunner));
        }

        public void Register(IPeriodicTask task, TimeSpan period)
        {
            lock (lockObject)
            {
                if (tasks.ContainsKey(task.Id))
                    return;
                var executor = new ThreadBasedPeriodicTaskExecutor(task, period, logger);
                executor.Start();
                tasks.Add(task.Id, executor);
            }
        }

        public void Unregister(string taskId, int timeout)
        {
            ThreadBasedPeriodicTaskExecutor taskRunner;
            lock (lockObject)
            {
                if (!tasks.ContainsKey(taskId))
                    return;
                taskRunner = tasks[taskId];
                tasks.Remove(taskId);
            }
            taskRunner.StopAndWait(TimeSpan.FromMilliseconds(timeout));
        }

        public void Dispose()
        {
            try
            {
                logger.Info("Call Dispose method on ThreadBasedPeriodicTaskRunner");
                var stopTasks = new List<Task>();
                const int stoppingTimeout = 15000;
                lock (lockObject)
                {
                    var keys = tasks.Keys;
                    foreach (var taskId in keys)
                    {
                        var id = taskId;
                        stopTasks.Add(Task.Factory.StartNew(() =>
                            {
                                try
                                {
                                    Unregister(id, stoppingTimeout);
                                }
                                catch (Exception e)
                                {
                                    logger.Error(e, "Error during unregister periodic task with id '{TaskId}'", new {TaskId = id});
                                }
                            }));
                    }
                }
                Task.WaitAll(stopTasks.ToArray(), stoppingTimeout);
                if (stopTasks.Any(task => task.Status == TaskStatus.Running))
                    throw new StoppingTimeoutException(stoppingTimeout);
                logger.Info("ThreadBasedPeriodicTaskRunner successfully disposed");
            }
            catch (Exception e)
            {
                logger.Error(e, "Error during dispose ThreadBasedPeriodicTaskRunner");
            }
        }

        private readonly ILog logger;
        private readonly Dictionary<string, ThreadBasedPeriodicTaskExecutor> tasks = new Dictionary<string, ThreadBasedPeriodicTaskExecutor>();
        private readonly object lockObject = new object();
    }
}