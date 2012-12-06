using System;
using System.Threading;

using NUnit.Framework;

using RemoteQueue.Cassandra.RemoteLock;

using log4net;

namespace FunctionalTests.RemoteLock
{
    public class RemoteLockTest : ThreadsTestBase
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
            DoTestIncrementDecrementLock(30, 60000);
        }

        [Test]
        public void TestIncrementDecrementLock()
        {
            DoTestIncrementDecrementLock(10, 10000);
        }

        private void DoTestIncrementDecrementLock(int threadCount, int timeInterval)
        {
            for(int i = 0; i < threadCount; i++)
                AddThread(IncrementDecrementAction);
            RunThreads(timeInterval);
            JoinThreads();
        }

        private void IncrementDecrementAction(Random random)
        {
            using(var remoteLock = lockCreator.Lock(lockId))
            {
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
            var lr = container.Get<LockRepository>();
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