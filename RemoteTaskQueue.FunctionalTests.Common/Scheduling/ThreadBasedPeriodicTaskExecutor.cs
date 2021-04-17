using System;
using System.Diagnostics;
using System.Threading;

using Vostok.Logging.Abstractions;

namespace RemoteTaskQueue.FunctionalTests.Common.Scheduling
{
    internal class ThreadBasedPeriodicTaskExecutor
    {
        public ThreadBasedPeriodicTaskExecutor(IPeriodicTask task, TimeSpan period, ILog logger)
        {
            this.task = task;
            this.period = period;
            this.logger = logger;
        }

        public void Start()
        {
            stopEvent = new ManualResetEventSlim(false);
            thread = new Thread(TaskExecutionProc)
                {
                    Name = task.Id,
                    IsBackground = true,
                };
            thread.Start();
        }

        public void StopAndWait(TimeSpan timeout)
        {
            stopEvent.Set();
            if (!thread.Join(timeout))
                throw new InvalidOperationException($"Can not stop task for {timeout}");
        }

        private void TaskExecutionProc()
        {
            Stopwatch iterationStopwatch;
            do
            {
                iterationStopwatch = Stopwatch.StartNew();
                try
                {
                    logger.Debug($"Start run task '{task.Id}'");
                    task.Run();
                    logger.Debug($"Finish run task '{task.Id}'");
                }
                catch (Exception e)
                {
                    logger.Error(e, "Error while executing task {TaskId}", new {TaskId = task.Id});
                }
            } while (!stopEvent.Wait(DateTimeMath.Max(TimeSpan.Zero, period - iterationStopwatch.Elapsed)));
        }

        private readonly IPeriodicTask task;
        private readonly TimeSpan period;
        private Thread thread;
        private ManualResetEventSlim stopEvent;
        private readonly ILog logger;
    }
}