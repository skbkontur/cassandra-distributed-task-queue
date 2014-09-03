using System;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Profiling
{
    public interface IGraphiteRemoteTaskQueueProfilerSettings
    {
        TimeSpan AggregationPeriod { get; }
    }
}