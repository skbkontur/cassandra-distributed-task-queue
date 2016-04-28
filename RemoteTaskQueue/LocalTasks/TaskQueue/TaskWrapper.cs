using System;
using System.Collections.Concurrent;
using System.Diagnostics;

using GroboTrace;

using JetBrains.Annotations;

using log4net;

using RemoteQueue.Handling;

using SKBKontur.Catalogue.ServiceLib.Tracing;

namespace RemoteQueue.LocalTasks.TaskQueue
{
    internal class TaskWrapper
    {
        public TaskWrapper([NotNull] string taskId, [CanBeNull] string taskName, TaskQueueReason taskQueueReason, bool taskIsBeingTraced, [NotNull] HandlerTask handlerTask, [NotNull] LocalTaskQueue localTaskQueue)
        {
            this.taskId = taskId;
            this.taskName = taskName ?? "Unknown";
            this.taskQueueReason = taskQueueReason;
            this.taskIsBeingTraced = taskIsBeingTraced;
            this.handlerTask = handlerTask;
            this.localTaskQueue = localTaskQueue;
            finished = false;
        }

        public bool Finished { get { return finished; } }

        public void Run()
        {
            LocalTaskProcessingResult result;
            var stopwatch = Stopwatch.StartNew();
            TracingAnalyzer.ClearStats();
            try
            {
                result = handlerTask.RunTask();
            }
            catch(Exception e)
            {
                result = LocalTaskProcessingResult.Undefined;
                logger.Error("Ошибка во время обработки асинхронной задачи.", e);
            }
            try
            {
                var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                finished = true;
                localTaskQueue.TaskFinished(taskId, taskQueueReason, taskIsBeingTraced, result);
                double quantile;
                if (timeStatistics.GetOrAdd(taskName, s => new TimeStatistics(s)).AddTime(elapsedMilliseconds, out quantile))
                    profileLogger.Info(taskName + TracingAnalyzerStatsFormatter.Format(TracingAnalyzer.GetStats(), elapsedMilliseconds, quantile));
            }
            catch(Exception e)
            {
                logger.Warn("Ошибка во время окончания задачи.", e);
            }
        }

        private readonly string taskId;
        private readonly string taskName;
        private readonly TaskQueueReason taskQueueReason;
        private readonly bool taskIsBeingTraced;
        private readonly HandlerTask handlerTask;
        private readonly LocalTaskQueue localTaskQueue;
        private volatile bool finished;
        private readonly ILog logger = LogManager.GetLogger(typeof(LocalTaskQueue));
        private static readonly ILog profileLogger = LogManager.GetLogger(typeof(TaskWrapper).Assembly, "ProfileLogger");
        private static readonly ConcurrentDictionary<string, TimeStatistics> timeStatistics = new ConcurrentDictionary<string, TimeStatistics>();
    }
}