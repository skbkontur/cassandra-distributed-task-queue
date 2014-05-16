namespace RemoteQueue.Handling
{
    public class SingleTaskCounter
    {
        public SingleTaskCounter(int maxCount)
        {
            this.maxCount = maxCount;
            needLock = maxCount > 0;
        }

        public bool CanQueueTask()
        {
            if(!needLock) return true;
            lock(lockObject)
            {
                return count < maxCount;
            }
        }

        public bool TryIncrement()
        {
            if(!needLock) return true;
            lock(lockObject)
            {
                if(count < maxCount)
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
        private readonly int maxCount;
        private readonly bool needLock;
    }
}