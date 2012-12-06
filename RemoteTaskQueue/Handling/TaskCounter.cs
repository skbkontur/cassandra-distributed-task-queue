using RemoteQueue.Settings;

namespace RemoteQueue.Handling
{
    public class TaskCounter : ITaskCounter
    {
        public TaskCounter(IExchangeSchedulableRunnerSettings settings)
        {
            this.settings = settings;
            needLock = settings.MaxRunningTasksCount > 0;
        }

        public bool CanQueueTask()
        {
            if(!needLock) return true;
            lock(lockObject)
            {
                return count < settings.MaxRunningTasksCount;
            }
        }

        public bool TryIncrement()
        {
            if(!needLock) return true;
            lock(lockObject)
            {
                if(count < settings.MaxRunningTasksCount)
                {
                    count++;
                    return true;
                }
                return false;
            }
        }

        public void Decrement()
        {
            if(!needLock) return;
            lock(lockObject)
            {
                count--;
            }
        }

        private volatile int count;
        private readonly object lockObject = new object();

        private readonly IExchangeSchedulableRunnerSettings settings;
        private readonly bool needLock;
    }
}