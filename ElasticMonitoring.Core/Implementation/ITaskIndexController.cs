using System;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Types;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation
{
    public interface ITaskIndexController : IDisposable
    {
        void ProcessNewEvents();
        void SetMinTicksHack(long minTicks);
        bool IsDistributedLockAcquired();
        ElasticMonitoringStatus GetStatus();
        long MinTicksHack { get; }
        void SendActualizationLagToGraphite();
    }
}