using System;

namespace RemoteTaskQueue.FunctionalTests.Common.Scheduling
{
    public class StoppingTimeoutException : Exception
    {
        public StoppingTimeoutException(int timeout)
            : base($"One of tasks have not stopped during {timeout / 1000}.{timeout % 1000:000}s")
        {
        }
    }
}