using System;

using RemoteQueue.Settings;

namespace RemoteQueue.Handling
{
    public class TaskCounter : ITaskCounter
    {
        public TaskCounter(IExchangeSchedulableRunnerSettings settings)
        {
            pullFromQueueCounter = new SingleTaskCounter(settings.MaxRunningTasksCount);
            continuationCounter = new SingleTaskCounter(0);
        }

        public bool CanQueueTask(TaskQueueReason reason)
        {
            return GetCounter(reason).CanQueueTask();
        }

        public bool TryIncrement(TaskQueueReason reason)
        {
            return GetCounter(reason).TryIncrement();
        }

        public void Decrement(TaskQueueReason reason)
        {
            GetCounter(reason).Decrement();
        }

        private SingleTaskCounter GetCounter(TaskQueueReason reason)
        {
            if(reason == TaskQueueReason.PullFromQueue)
                return pullFromQueueCounter;
            if(reason == TaskQueueReason.TaskContinuation)
                return continuationCounter;
            throw new NotSupportedException(string.Format("Unknown reason of task queueing: {0}", reason));
        }

        private readonly SingleTaskCounter pullFromQueueCounter;
        private readonly SingleTaskCounter continuationCounter;
    }
}