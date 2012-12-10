using System;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringService.Sheduler
{
    public interface IMonitoringSchedulableRunnerSettings
    {
        TimeSpan PeriodicInterval { get; }
    }
}