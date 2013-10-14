using System;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Settings
{
    public interface IMonitoringServiceSettings
    {
        TimeSpan PeriodicInterval { get; }
        bool ActualizeOnQuery { get; }
    }
}