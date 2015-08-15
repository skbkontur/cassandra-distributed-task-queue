using System;

using log4net;

using RemoteQueue.Handling;

namespace RemoteQueue.LocalTasks.TaskQueue
{
    public class TaskWrapper
    {
        public TaskWrapper(HandlerTask handlerTask, TaskQueue taskQueue)
        {
            this.handlerTask = handlerTask;
            this.taskQueue = taskQueue;
            finished = false;
        }

        public void Run()
        {
            try
            {
                handlerTask.RunTask();
            }
            catch(Exception e)
            {
                logger.Error("Ошибка во время обработки асинхронной задачи.", e);
            }
            try
            {
                finished = true;
                taskQueue.TaskFinished(handlerTask.TaskId);
            }
            catch(Exception e)
            {
                logger.Warn("Ошибка во время окончания задачи.", e);
            }
        }

        public bool Finished { get { return finished; } }
        private readonly HandlerTask handlerTask;
        private readonly TaskQueue taskQueue;
        private volatile bool finished;
        private readonly ILog logger = LogManager.GetLogger(typeof(TaskQueue));
    }
}