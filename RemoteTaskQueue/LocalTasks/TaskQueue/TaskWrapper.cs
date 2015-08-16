using System;

using log4net;

using RemoteQueue.Handling;

namespace RemoteQueue.LocalTasks.TaskQueue
{
    public class TaskWrapper
    {
        public TaskWrapper(string taskId, HandlerTask handlerTask, LocalTaskQueue localTaskQueue)
        {
            this.taskId = taskId;
            this.handlerTask = handlerTask;
            this.localTaskQueue = localTaskQueue;
            finished = false;
        }

        public bool Finished { get { return finished; } }

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
                localTaskQueue.TaskFinished(taskId);
            }
            catch(Exception e)
            {
                logger.Warn("Ошибка во время окончания задачи.", e);
            }
        }

        private readonly string taskId;
        private readonly HandlerTask handlerTask;
        private readonly LocalTaskQueue localTaskQueue;
        private volatile bool finished;
        private readonly ILog logger = LogManager.GetLogger(typeof(LocalTaskQueue));
    }
}