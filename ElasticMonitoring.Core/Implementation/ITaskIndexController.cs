using System;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation
{
    public interface ITaskIndexController : IDisposable
    {
        void ProcessNewEvents();
        void SetMinTicksHack(long minTicks);
        bool IsDistributedLockAcquired();
        long MinTicksHack { get; }
    }
}