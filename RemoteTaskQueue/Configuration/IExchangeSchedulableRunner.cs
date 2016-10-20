using System;

using RemoteQueue.Handling;

namespace RemoteQueue.Configuration
{
    public interface IExchangeSchedulableRunner
    {
        void Start();
        void Stop();

        [Obsolete("Only for usage in tests")]
        IRemoteTaskQueueBackdoor RemoteTaskQueueBackdoor { get; }
    }
}