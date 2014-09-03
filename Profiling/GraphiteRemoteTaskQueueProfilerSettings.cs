using System;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Profiling
{
    public class GraphiteRemoteTaskQueueProfilerSettings : IGraphiteRemoteTaskQueueProfilerSettings
    {
        public TimeSpan AggregationPeriod { get { return TimeSpan.FromMinutes(1); } }
    }
}