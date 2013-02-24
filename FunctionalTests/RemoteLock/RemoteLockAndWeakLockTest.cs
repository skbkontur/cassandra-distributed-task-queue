using System;
using System.Threading;

using NUnit.Framework;

using RemoteLock;

using log4net;

namespace FunctionalTests.RemoteLock
{
    public class RemoteLockAndWeakLockTest : ThreadsTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            lockCreator = container.Get<IRemoteLockCreator>();
            logger = LogManager.GetLogger(typeof(RemoteLockTest));
        }

        [Test, Ignore("Очень жирный тест")]
        public void StressTest()
        {
            DoTestIncrementDecrementLock(60, 60000);
        }

        [Test]
        public void TestIncrementDecrementLock()
        {
            DoTestIncrementDecrementLock(10, 10000);
        }

        private void DoTestIncrementDecrementLock(int threadCount, int timeInterval)
        {
            for(int i = 0; i < threadCount/2; i++)
                AddThread(IncrementDecrementActionLock);
            for (int i = threadCount/2; i < threadCount; i++)
                AddThread(IncrementDecrementActionWeakLock);
            RunThreads(timeInterval);
            JoinThreads();
        }

        private void IncrementDecrementActionLock(Random random)
        {
            using(var remoteLock = lockCreator.Lock(lockId))
            {
                Thread.Sleep(random.Next(5000));
                logger.Info("MakeLock with threadId: " + remoteLock.ThreadId);
                CheckLocks(remoteLock.ThreadId);
                Assert.AreEqual(0, ReadX());
                logger.Info("Increment");
                Interlocked.Increment(ref x);
                logger.Info("Decrement");
                Interlocked.Decrement(ref x);
            }
        }

        private void IncrementDecrementActionWeakLock(Random random)
        {
            IRemoteLock remoteLock;
            if(!lockCreator.TryGetLock(lockId, out remoteLock))
                return;
            using(remoteLock)
            {
                Thread.Sleep(random.Next(5000));
                logger.Info("MakeLock with threadId: " + remoteLock.ThreadId);
                CheckLocks(remoteLock.ThreadId);
                Assert.AreEqual(0, ReadX());
                logger.Info("Increment");
                Interlocked.Increment(ref x);
                logger.Info("Decrement");
                Interlocked.Decrement(ref x);
            }
        }

        private int ReadX()
        {
            return Interlocked.CompareExchange(ref x, 0, 0);
        }

        private void CheckLocks(string threadId)
        {
            var lr = container.Get<ILockRepository>();
            var locks = lr.GetLockThreads(lockId);
            logger.Info("Locks: " + string.Join(", ", locks));
            Assert.That(locks.Length <= 1, "Too many locks");
            Assert.That(locks.Length == 1);
            Assert.AreEqual(threadId, locks[0]);
            var lockShades = lr.GetShadeThreads(lockId);
            logger.Info("LockShades: " + string.Join(", ", lockShades));
        }

        private int x;
        private IRemoteLockCreator lockCreator;
        private ILog logger;

        private const string lockId = "IncDecLock";
    }
}