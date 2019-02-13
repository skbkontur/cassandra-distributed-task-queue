using System;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using RemoteTaskQueue.Monitoring.Storage;
using RemoteTaskQueue.Monitoring.Storage.Writing;

using SkbKontur.Graphite.Client;

using SKBKontur.Catalogue.Core.EventFeeds;
using SKBKontur.Catalogue.Core.EventFeeds.Building;

using Vostok.Logging.Abstractions;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class RtqMonitoringEventFeeder
    {
        public RtqMonitoringEventFeeder(ILog logger,
                                        EventFeedFactory eventFeedFactory,
                                        RtqElasticsearchIndexerSettings indexerSettings,
                                        IRtqElasticsearchClient elasticsearchClient,
                                        RemoteQueue.Handling.RemoteTaskQueue remoteTaskQueue,
                                        IStatsDClient statsDClient)
        {
            this.logger = logger.ForContext("CassandraDistributedTaskQueue.Monitoring");
            this.eventFeedFactory = eventFeedFactory;
            GlobalTime = remoteTaskQueue.GlobalTime;
            globalTimeProvider = new RtqGlobalTimeProvider(GlobalTime);
            eventLogRepository = remoteTaskQueue.EventLogRepository;
            var perfGraphiteReporter = new RtqMonitoringPerfGraphiteReporter("SubSystem.RemoteTaskQueue.ElasticsearchIndexer", statsDClient);
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
            const string key = "RtqMonitoring";
            return eventFeedFactory
                .WithOffsetType<string>()
                .WithEventType(BladesBuilder.New(eventLogRepository, eventConsumer, logger)
                                            .WithBlade($"{key}_Blade0", delay : TimeSpan.FromMinutes(1))
                                            .WithBlade($"{key}_Blade1", delay : TimeSpan.FromMinutes(15)))
                .WithGlobalTimeProvider(globalTimeProvider)
                .WithOffsetInterpreter(offsetInterpreter)
                .WithOffsetStorageFactory(bladeId => new RtqElasticsearchOffsetStorage(elasticsearchClient, offsetInterpreter, bladeId.BladeKey))
                .RunFeeds(delayBetweenIterations : TimeSpan.FromMinutes(1));
        }

        private readonly ILog logger;
        private readonly EventFeedFactory eventFeedFactory;
        private readonly RtqGlobalTimeProvider globalTimeProvider;
        private readonly EventLogRepository eventLogRepository;
        private readonly RtqMonitoringEventConsumer eventConsumer;
        private readonly RtqEventLogOffsetInterpreter offsetInterpreter;
        private readonly IRtqElasticsearchClient elasticsearchClient;
    }
}