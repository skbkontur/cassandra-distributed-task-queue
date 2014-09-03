using System;
using System.Collections.Concurrent;

using GroboContainer.Infection;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling.HandlerResults;
using RemoteQueue.Profiling;

using SKBKontur.Catalogue.Core.DistributedEventsProfilingClient;
using SKBKontur.Catalogue.Core.DistributedEventsProfilingClient.DataTypes;
using SKBKontur.Catalogue.Core.GraphitePeriodicSender.PeriodicSender;
using SKBKontur.Catalogue.Core.GraphitePeriodicSender.StatisticsSources;
using SKBKontur.Catalogue.StatisticsAggregator;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Profiling
{
    [IgnoredImplementation]
    public class GraphiteRemoteTaskQueueProfiler : IRemoteTaskQueueProfiler
    {
        public GraphiteRemoteTaskQueueProfiler(IGraphiteRemoteTaskQueueProfilerSettings settings, IGraphitePeriodicSender graphitePeriodicSender, IDistributedEventsProfiler distributedEventsProfiler)
        {
            this.graphitePeriodicSender = graphitePeriodicSender;
            this.distributedEventsProfiler = distributedEventsProfiler;
            statisticsAggregators = new ConcurrentDictionary<string, IStatisticsAggregator>();
            aggregationPeriod = settings.AggregationPeriod;
        }

        public void ProcessTaskEnqueueing(TaskMetaInformation meta)
        {
            distributedEventsProfiler.LogEvent(string.Format("{0}_{1}", meta.Id, meta.Attempts + 1), GetStatisticsKey(meta), DistributedEventType.Start);
        }

        public void ProcessTaskDequeueing(TaskMetaInformation meta)
        {
            distributedEventsProfiler.LogEvent(string.Format("{0}_{1}", meta.Id, meta.Attempts), GetStatisticsKey(meta), DistributedEventType.Finish);
        }

        public void RecordTaskExecutionTime(TaskMetaInformation meta, TimeSpan taskExecutionTime)
        {
            IStatisticsAggregator aggregator;
            if(!statisticsAggregators.TryGetValue(meta.Name, out aggregator))
            {
                lock(lockObject)
                {
                    if(!statisticsAggregators.TryGetValue(meta.Name, out aggregator))
                    {
                        aggregator = new StatisticsAggregator.StatisticsAggregator(aggregationPeriod, TimeSpan.FromTicks(aggregationPeriod.Ticks * 5));
                        var avgStatisticsName = string.Format("EDI.services.RemoteTaskQueue.{0}.TaskExecutionTime.{1}_avg", Environment.MachineName, meta.Name);
                        graphitePeriodicSender.AddSource(new AverageStatisticsSource(aggregator, avgStatisticsName, aggregationPeriod), aggregationPeriod);
                        var quantileStatisticsName = string.Format("EDI.services.RemoteTaskQueue.{0}.TaskExecutionTime.{1}_95", Environment.MachineName, meta.Name);
                        graphitePeriodicSender.AddSource(new QuantileStatisticsSource(aggregator, quantileStatisticsName, aggregationPeriod, 95), aggregationPeriod);
                        if(!statisticsAggregators.TryAdd(meta.Name, aggregator))
                            throw new Exception("Some strange concurrent behaviour. Should never happen");
                    }
                }
            }
            aggregator.AddEvent((int)taskExecutionTime.TotalMilliseconds);
        }

        private static string GetStatisticsKey(TaskMetaInformation meta)
        {
            return string.Format("EDI.services.RemoteTaskQueue.TaskWaitingForStartTime.{0}", meta.Name);
        }

        private readonly ConcurrentDictionary<string, IStatisticsAggregator> statisticsAggregators;
        private readonly IGraphitePeriodicSender graphitePeriodicSender;
        private readonly IDistributedEventsProfiler distributedEventsProfiler;
        private TimeSpan aggregationPeriod;
        private readonly object lockObject = new object();
    }
}