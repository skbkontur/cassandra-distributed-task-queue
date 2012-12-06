using System;

using log4net;

namespace RemoteQueue.LocalTasks.TaskQueue
{
    public class TaskWrapper
    {
        public TaskWrapper(ITask task, TaskQueue taskQueue)
        {
            this.task = task;
            this.taskQueue = taskQueue;
            finished = false;
        }

        public void Run()
        {
            var result = new TaskResult();
            try
            {
                result = task.RunTask();
            }
            catch(Exception e)
            {
                logger.Error(string.Format("Ошибка во время обработки асинхронной задачи."), e);
            }
            try
            {
                finished = true;
                taskQueue.TaskFinished(task);
                if (result == TaskResult.Rerun)
                    taskQueue.QueueTask(task);
            }
            catch(Exception e)
            {
                logger.Warn(string.Format("Ошибка во время окончания задачи."), e);
            }
        }

        public bool Finished { get { return finished; } }

        private readonly ITask task;
        private readonly TaskQueue taskQueue;
        private volatile bool finished;
        private readonly ILog logger = LogManager.GetLogger(typeof(TaskQueue));
    }
}