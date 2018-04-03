using System;

namespace RemoteQueue.Handling
{
    public class TaskCounter : ITaskCounter
    {
        public TaskCounter(int maxRunningTasksCount, int maxRunningContinuationsCount)
        {
            this.maxRunningTasksCount = maxRunningTasksCount;
            this.maxRunningContinuationsCount = maxRunningContinuationsCount;
        }

        public bool CanQueueTask(TaskQueueReason reason)
        {
            if (reason == TaskQueueReason.TaskContinuation)
            {
                if (maxRunningContinuationsCount == 0)
                    return true;
                return continuationsCount < maxRunningContinuationsCount;
            }
            if (reason == TaskQueueReason.PullFromQueue)
            {
                if (maxRunningTasksCount == 0)
                    return true;
                if (maxRunningContinuationsCount == 0)
                    return (continuationsCount + tasksCount) < (maxRunningTasksCount);
                else
                    return (tasksCount) < (maxRunningTasksCount);
            }
            throw new InvalidOperationException(string.Format("Неизвестный тип TaskQueueReason: {0}", reason));
        }

        public bool TryIncrement(TaskQueueReason reason)
        {
            if (reason == TaskQueueReason.TaskContinuation)
            {
                lock (lockObject)
                {
                    if (CanQueueTask(reason))
                    {
                        continuationsCount++;
                        return true;
                    }
                    return maxRunningContinuationsCount == 0;
                }
            }
            else if (reason == TaskQueueReason.PullFromQueue)
            {
                lock (lockObject)
                {
                    if (CanQueueTask(reason))
                    {
                        tasksCount++;
                        return true;
                    }
                    return maxRunningTasksCount == 0;
                }
            }
            throw new InvalidOperationException(string.Format("Неизвестный тип TaskQueueReason: {0}", reason));
        }

        public void Decrement(TaskQueueReason reason)
        {
            if (reason == TaskQueueReason.TaskContinuation)
            {
                lock (lockObject)
                {
                    continuationsCount--;
                }
            }
            else if (reason == TaskQueueReason.PullFromQueue)
            {
                lock (lockObject)
                {
                    tasksCount--;
                }
            }
            else
                throw new InvalidOperationException(string.Format("Неизвестный тип TaskQueueReason: {0}", reason));
        }

        private readonly int maxRunningTasksCount;
        private readonly int maxRunningContinuationsCount;
        private int tasksCount;
        private int continuationsCount;
        private readonly object lockObject = new object();
    }
}