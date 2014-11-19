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
        public GraphiteRemoteTaskQueueProfiler(
            IGraphiteRemoteTaskQueueProfilerSettings settings, 
            IGraphitePeriodicSender graphitePeriodicSender, 
            IDistributedEventsProfiler distributedEventsProfiler)
        {
            this.settings = settings;
            this.graphitePeriodicSender = graphitePeriodicSender;
            this.distributedEventsProfiler = distributedEventsProfiler;
            statisticsAggregators = new ConcurrentDictionary<string, IStatisticsAggregator>();
            aggregationPeriod = settings.AggregationPeriod;
        }

        public void ProcessTaskCreation(TaskMetaInformation meta)
        {
            var statisticsKey = FormatKeyName("{0}.TasksCreated.{1}", Environment.MachineName, meta.Name);
            distributedEventsProfiler.LogEvent(meta.Id, statisticsKey, DistributedEventType.Atomic);
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
                        var avgStatisticsName = FormatKeyName("{0}.TaskExecutionTime.{1}.Average", Environment.MachineName, meta.Name);
                        graphitePeriodicSender.AddSource(new AverageStatisticsSource(aggregator, avgStatisticsName, aggregationPeriod), aggregationPeriod);
                        var quantileStatisticsName = FormatKeyName("{0}.TaskExecutionTime.{1}.Quantile95", Environment.MachineName, meta.Name);
                        graphitePeriodicSender.AddSource(new QuantileStatisticsSource(aggregator, quantileStatisticsName, aggregationPeriod, 95), aggregationPeriod);
                        if(!statisticsAggregators.TryAdd(meta.Name, aggregator))
                            throw new Exception("Some strange concurrent behaviour. Should never happen");
                    }
                }
            }
            aggregator.AddEvent((int)taskExecutionTime.TotalMilliseconds);
        }

        public void RecordTaskExecutionResult(TaskMetaInformation meta, HandleResult handleResult)
        {
            IStatisticsAggregator aggregator;
            var dictionaryKey = string.Format("{0}_{1}", meta.Name, handleResult.FinishAction);
            if(!statisticsAggregators.TryGetValue(dictionaryKey, out aggregator))
            {
                lock(lockObject)
                {
                    if(!statisticsAggregators.TryGetValue(dictionaryKey, out aggregator))
                    {
                        aggregator = new StatisticsAggregator.StatisticsAggregator(aggregationPeriod, TimeSpan.FromTicks(aggregationPeriod.Ticks * 5));
                        var statisticsName = FormatKeyName("{0}.NumberOfExecutedTasks.{1}.{2}", Environment.MachineName, meta.Name, handleResult.FinishAction);
                        graphitePeriodicSender.AddSource(new AmountStatisticsSource(aggregator, statisticsName, aggregationPeriod), aggregationPeriod);
                        if(!statisticsAggregators.TryAdd(dictionaryKey, aggregator))
                            throw new Exception("Some strange concurrent behaviour. Should never happen");
                    }
                }
            }
            aggregator.AddEvent(0);
        }

        private string FormatKeyName(string format, params object[] args)
        {
            return settings.KeyNamePrefix + "." + string.Format(format, args);
        }

        private string GetStatisticsKey(TaskMetaInformation meta)
        {
            return FormatKeyName("TaskWaitingForStartTime.{0}", meta.Name);
        }

        private readonly ConcurrentDictionary<string, IStatisticsAggregator> statisticsAggregators;
        private readonly IGraphiteRemoteTaskQueueProfilerSettings settings;
        private readonly IGraphitePeriodicSender graphitePeriodicSender;
        private readonly IDistributedEventsProfiler distributedEventsProfiler;
        private TimeSpan aggregationPeriod;
        private readonly object lockObject = new object();
    }
}