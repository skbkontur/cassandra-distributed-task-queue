using System;

using GroboTrace;

using JetBrains.Annotations;

using log4net;

using RemoteQueue.Handling;
using RemoteQueue.Profiling;

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
            LocalTaskProcessingResult result;
            try
            {
                using(Profiler.Profile(groboTraceKey, new RtqGroboTraceProfilerSink(taskId)))
                    result = handlerTask.RunTask();
            }
            catch(Exception e)
            {
                result = LocalTaskProcessingResult.Undefined;
                logger.Error("Ошибка во время обработки асинхронной задачи.", e);
            }
            try
            {
                finished = true;
                localTaskQueue.TaskFinished(taskId, taskQueueReason, taskIsBeingTraced, result);
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
    }
}