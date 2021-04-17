using System;

namespace RemoteTaskQueue.FunctionalTests.RemoteTaskQueue.ExchangeTests
{
    public class TooLateException : Exception
    {
        public TooLateException(string format, params object[] parameters)
            : base(string.Format(format, parameters))
        {
        }
    }
}