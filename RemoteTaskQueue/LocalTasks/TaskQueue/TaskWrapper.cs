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
        public TaskWrapper([NotNull] string taskId, [NotNull] string groboTraceKey, TaskQueueReason taskQueueReason, bool taskIsBeingTraced, [NotNull] HandlerTask handlerTask, [NotNull] LocalTaskQueue localTaskQueue)
        {
            this.taskId = taskId;
            this.groboTraceKey = groboTraceKey;
            this.taskQueueReason = taskQueueReason;
            this.taskIsBeingTraced = taskIsBeingTraced;
            this.handlerTask = handlerTask;
            this.localTaskQueue = localTaskQueue;
            finished = false;
        }

        public bool Finished { get { return finished; } }

        public void Run()
        {
            TracingAnalyzer.ClearStats();
            var sw = Stopwatch.StartNew();
            LocalTaskProcessingResult result;
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
                sw.Stop();
                finished = true;
                localTaskQueue.TaskFinished(taskId, taskQueueReason, taskIsBeingTraced, result);
                double quantile;
                if(timeStatistics.GetOrAdd(groboTraceKey, s => new TimeStatistics(s)).AddTime(sw.ElapsedMilliseconds, out quantile))
                    ProfileLogger.Instance.Info(string.Format("GroboTraceKey: {0}, TracingStats: {1}", groboTraceKey, TracingAnalyzerStatsFormatter.Format(TracingAnalyzer.GetStats(), sw.ElapsedMilliseconds, quantile)));
            }
            catch(Exception e)
            {
                logger.Warn("Ошибка во время окончания задачи.", e);
            }
        }

        private readonly string taskId;
        private readonly string groboTraceKey;
        private readonly TaskQueueReason taskQueueReason;
        private readonly bool taskIsBeingTraced;
        private readonly HandlerTask handlerTask;
        private readonly LocalTaskQueue localTaskQueue;
        private volatile bool finished;
        private readonly ILog logger = LogManager.GetLogger(typeof(LocalTaskQueue));
        private static readonly ConcurrentDictionary<string, TimeStatistics> timeStatistics = new ConcurrentDictionary<string, TimeStatistics>();
    }
}