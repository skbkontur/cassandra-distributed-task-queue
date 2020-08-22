using System;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Scheduling
{
    public class StoppingTimeoutException : Exception
    {
        public StoppingTimeoutException(int timeout)
            : base(string.Format("One of tasks have not stopped during {0}.{1:000}s", timeout / 1000, timeout % 1000))
        {
        }
    }
}