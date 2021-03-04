using System;
using System.Threading;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.EventFeed;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage.Writing;
using SkbKontur.Cassandra.GlobalTimestamp;
using SkbKontur.EventFeeds;
using SkbKontur.Graphite.Client;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Indexer
{
    [PublicAPI]
    public class RtqMonitoringEventFeeder : IRtqMonitoringEventFeeder
    {
        public RtqMonitoringEventFeeder(ILog logger,
                                        RtqElasticsearchIndexerSettings indexerSettings,
                                        IRtqElasticsearchClient elasticsearchClient,
                                        IGraphiteClient graphiteClient,
                                        IStatsDClient statsDClient,
                                        IRtqPeriodicJobRunner rtqPeriodicJobRunner,
                                        RemoteTaskQueue remoteTaskQueue)
        {
            this.logger = logger.ForContext("CassandraDistributedTaskQueue").ForContext(nameof(RtqMonitoringEventFeeder));
            this.indexerSettings = indexerSettings;
            this.elasticsearchClient = elasticsearchClient;
            GlobalTime = remoteTaskQueue.GlobalTime;
            eventSource = new RtqEventSource(remoteTaskQueue.EventLogRepository);
            var eventFeedPeriodicJobRunner = new RtqEventFeedPeriodicJobRunner(rtqPeriodicJobRunner, graphiteClient, indexerSettings.EventFeedGraphitePathPrefix);
            eventFeedFactory = new EventFeedFactory(new RtqEventFeedGlobalTimeProvider(GlobalTime), eventFeedPeriodicJobRunner);
            var perfGraphiteReporter = new RtqMonitoringPerfGraphiteReporter(statsDClient, indexerSettings.PerfGraphitePathPrefix);
            var taskMetaProcessor = new TaskMetaProcessor(this.logger, indexerSettings, elasticsearchClient, remoteTaskQueue, perfGraphiteReporter);
            eventConsumer = new RtqMonitoringEventConsumer(indexerSettings, taskMetaProcessor);
        }

        [NotNull]
        public IGlobalTime GlobalTime { get; }

        [NotNull]
        public IEventFeedsRunner RunEventFeeding(CancellationToken cancellationToken)
        {
            return eventFeedFactory
                   .WithOffsetType<string>()
                   .WithEventType(BladesBuilder.New(eventSource, eventConsumer, logger)
                                               .WithBlade($"{indexerSettings.EventFeedKey}_Blade0", delay : TimeSpan.FromMinutes(1))
                                               .WithBlade($"{indexerSettings.EventFeedKey}_Blade1", delay : TimeSpan.FromMinutes(15)))
                   .WithOffsetInterpreter(offsetInterpreter)
                   .WithOffsetStorageFactory(bladeId => new RtqElasticsearchOffsetStorage(elasticsearchClient, offsetInterpreter, bladeId.BladeKey))
                   .RunFeeds(delayBetweenIterations : TimeSpan.FromMinutes(1), cancellationToken);
        }

        private readonly ILog logger;
        private readonly RtqElasticsearchIndexerSettings indexerSettings;
        private readonly IRtqElasticsearchClient elasticsearchClient;
        private readonly RtqEventSource eventSource;
        private readonly EventFeedFactory eventFeedFactory;
        private readonly RtqMonitoringEventConsumer eventConsumer;
        private readonly RtqEventLogOffsetInterpreter offsetInterpreter = new RtqEventLogOffsetInterpreter();
    }
}