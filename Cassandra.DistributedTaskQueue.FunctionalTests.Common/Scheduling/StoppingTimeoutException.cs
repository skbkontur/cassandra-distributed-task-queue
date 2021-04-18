using System;

namespace SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.Scheduling
{
    public class StoppingTimeoutException : Exception
    {
        public StoppingTimeoutException(int timeout)
            : base($"One of tasks have not stopped during {timeout / 1000}.{timeout % 1000:000}s")
        {
        }
    }
}