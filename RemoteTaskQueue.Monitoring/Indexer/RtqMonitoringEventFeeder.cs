using System;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage.Writing;
using SkbKontur.Cassandra.GlobalTimestamp;

using SKBKontur.Catalogue.Core.EventFeeds;
using SKBKontur.Catalogue.Core.EventFeeds.Building;
using SKBKontur.Catalogue.Core.EventFeeds.Implementations;

using SkbKontur.Graphite.Client;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Indexer
{
    [PublicAPI]
    public class RtqMonitoringEventFeeder : IRtqMonitoringEventFeeder
    {
        public RtqMonitoringEventFeeder(ILog logger,
                                        EventFeedFactory eventFeedFactory,
                                        RtqElasticsearchIndexerSettings indexerSettings,
                                        IRtqElasticsearchClient elasticsearchClient,
                                        RemoteTaskQueue remoteTaskQueue,
                                        IStatsDClient statsDClient)
        {
            this.logger = logger.ForContext("CassandraDistributedTaskQueue").ForContext(nameof(RtqMonitoringEventFeeder));
            this.eventFeedFactory = eventFeedFactory;
            this.indexerSettings = indexerSettings;
            GlobalTime = remoteTaskQueue.GlobalTime;
            globalTimeProvider = new DefaultGlobalTimeProvider(GlobalTime);
            eventLogRepository = remoteTaskQueue.EventLogRepository;
            var perfGraphiteReporter = new RtqMonitoringPerfGraphiteReporter(indexerSettings.PerfGraphitePrefix, statsDClient);
            var taskMetaProcessor = new TaskMetaProcessor(this.logger, indexerSettings, elasticsearchClient, remoteTaskQueue, perfGraphiteReporter);
            eventConsumer = new RtqMonitoringEventConsumer(indexerSettings, taskMetaProcessor);
            offsetInterpreter = new RtqEventLogOffsetInterpreter();
            this.elasticsearchClient = elasticsearchClient;
        }

        [NotNull]
        public IGlobalTime GlobalTime { get; }

        [NotNull]
        public IEventFeedsRunner RunEventFeeding()
        {
            return eventFeedFactory
                   .WithOffsetType<string>()
                   .WithEventType(BladesBuilder.New(eventLogRepository, eventConsumer, logger)
                                               .WithBlade($"{indexerSettings.EventFeedKey}_Blade0", delay : TimeSpan.FromMinutes(1))
                                               .WithBlade($"{indexerSettings.EventFeedKey}_Blade1", delay : TimeSpan.FromMinutes(15)))
                   .WithGlobalTimeProvider(globalTimeProvider)
                   .WithOffsetInterpreter(offsetInterpreter)
                   .WithOffsetStorageFactory(bladeId => new RtqElasticsearchOffsetStorage(elasticsearchClient, offsetInterpreter, bladeId.BladeKey))
                   .RunFeeds(delayBetweenIterations : TimeSpan.FromMinutes(1));
        }

        private readonly ILog logger;
        private readonly EventFeedFactory eventFeedFactory;
        private readonly RtqElasticsearchIndexerSettings indexerSettings;
        private readonly IGlobalTimeProvider globalTimeProvider;
        private readonly EventLogRepository eventLogRepository;
        private readonly RtqMonitoringEventConsumer eventConsumer;
        private readonly RtqEventLogOffsetInterpreter offsetInterpreter;
        private readonly IRtqElasticsearchClient elasticsearchClient;
    }
}