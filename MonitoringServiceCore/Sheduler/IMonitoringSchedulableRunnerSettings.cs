using System;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Sheduler
{
    public interface IMonitoringSchedulableRunnerSettings
    {
        TimeSpan PeriodicInterval { get; }
    }
}