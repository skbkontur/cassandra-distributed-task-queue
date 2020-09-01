using System;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.LocalTasks.TaskQueue
{
    internal class TaskWrapper
    {
        public TaskWrapper([NotNull] string taskId,
                           TaskQueueReason taskQueueReason,
                           bool taskIsBeingTraced,
                           [NotNull] HandlerTask handlerTask,
                           [NotNull] LocalTaskQueue localTaskQueue,
                           [NotNull] ILog logger)
        {
            this.taskId = taskId;
            this.taskQueueReason = taskQueueReason;
            this.taskIsBeingTraced = taskIsBeingTraced;
            this.handlerTask = handlerTask;
            this.localTaskQueue = localTaskQueue;
            this.logger = logger;
            finished = false;
        }

        public bool Finished => finished;

        public void Run()
        {
            LocalTaskProcessingResult result;
            try
            {
                result = handlerTask.RunTask();
            }
            catch (Exception e)
            {
                result = LocalTaskProcessingResult.Undefined;
                logger.Error(e, "Ошибка во время обработки асинхронной задачи.");
            }
            try
            {
                finished = true;
                localTaskQueue.TaskFinished(taskId, taskQueueReason, taskIsBeingTraced, result);
            }
            catch (Exception e)
            {
                logger.Warn(e, "Ошибка во время окончания задачи.");
            }
        }

        private readonly string taskId;
        private readonly TaskQueueReason taskQueueReason;
        private readonly bool taskIsBeingTraced;
        private readonly HandlerTask handlerTask;
        private readonly LocalTaskQueue localTaskQueue;
        private readonly ILog logger;
        private volatile bool finished;
    }
}