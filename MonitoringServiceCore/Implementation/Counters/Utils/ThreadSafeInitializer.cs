using System;
using System.Threading;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation.Counters.Utils
{
    public class ThreadSafeInitializer
    {
        public ThreadSafeInitializer(Action initializeAction)
        {
            this.initializeAction = initializeAction;
        }

        public void EnsureInitialized()
        {
            if(!initilaized)
            {
                lock(initializeLock)
                    if(!initilaized)
                    {
                        initializeAction();
                        Thread.MemoryBarrier();
                        initilaized = true;
                    }
            }
        }

        private readonly Action initializeAction;
        private readonly object initializeLock = new object();
        private volatile bool initilaized;
    }
}