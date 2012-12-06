using System;
using System.Threading;

using log4net;

namespace RemoteQueue.Cassandra.RemoteLock
{
    public class WeakRemoteLock : IRemoteLock
    {
        public WeakRemoteLock(ILockRepository lockRepository, string lockId, out string concurrentThreadId, string threadId = null)
        {
            this.lockRepository = lockRepository;
            this.lockId = lockId;
            threadId = string.IsNullOrEmpty(threadId) ? Guid.NewGuid().ToString() : threadId;
            this.threadId = threadId;
            var random = new Random(Guid.NewGuid().GetHashCode());
            var attempt = 1;
            while(true)
            {
                var lockAttempt = lockRepository.TryLock(lockId, threadId);
                switch(lockAttempt.Status)
                {
                case LockAttemptStatus.Success:
                    stopEvent = new ManualResetEvent(false);
                    thread = new Thread(UpdateLock);
                    thread.Start();
                    concurrentThreadId = null;
                    return;
                case LockAttemptStatus.AnotherThreadIsOwner:
                    concurrentThreadId = lockAttempt.OwnerId;
                    return;
                default:
                    var shortSleep = random.Next(50 * (int)Math.Exp(Math.Min(attempt, 10)));
                    attempt++;
                    logger.WarnFormat("Поток {0} не смог взять блокировку {1} из-за конкуррентной попытки других потоков. Засыпаем на {2} миллисекунд.", threadId, lockId, shortSleep);
                    Thread.Sleep(shortSleep);
                    break;
                }
            }
        }

        public void Dispose()
        {
            if(stopEvent != null)
            {
                stopEvent.Set();
                thread.Join();
                stopEvent.Dispose();
                lockRepository.Unlock(lockId, threadId);
            }
        }

        public string LockId { get { return lockId; } }
        public string ThreadId { get { return threadId; } }

        private void UpdateLock()
        {
            var lastTicks = DateTime.UtcNow;
            while(true)
            {
                try
                {
                    var diff = DateTime.UtcNow - lastTicks;
                    if(diff > TimeSpan.FromSeconds(50))
                    {
                        logger.Error(string.Format("Difference between updates too large: {0}s. Update stopped", diff));
                        return;
                    }
                    lockRepository.Relock(lockId, threadId);
                    if(stopEvent.WaitOne(5000)) break;
                    diff = DateTime.UtcNow - lastTicks;
                    if(diff > TimeSpan.FromSeconds(30))
                        logger.WarnFormat(string.Format("Difference between updates too large: {0}s", diff));

                    lastTicks = DateTime.UtcNow;
                }
                catch(Exception e)
                {
                    logger.Error("Ошибка во время удержания блокировки.", e);
                }
            }
        }

        private readonly ILog logger = LogManager.GetLogger(typeof(WeakRemoteLock));
        private readonly string lockId;
        private readonly string threadId;
        private readonly ILockRepository lockRepository;
        private readonly Thread thread;
        private readonly ManualResetEvent stopEvent;
    }
}