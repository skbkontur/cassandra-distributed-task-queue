using System;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using RemoteTaskQueue.Monitoring.Storage;
using RemoteTaskQueue.Monitoring.Storage.Writing;

using SkbKontur.Graphite.Client;

using SKBKontur.Catalogue.Core.EventFeeds;
using SKBKontur.Catalogue.Core.EventFeeds.Building;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class RtqMonitoringEventFeeder
    {
        public RtqMonitoringEventFeeder(EventFeedFactory eventFeedFactory,
                                        RtqElasticsearchIndexerSettings indexerSettings,
                                        RtqElasticsearchClientFactory elasticsearchClientFactory,
                                        RemoteQueue.Handling.RemoteTaskQueue remoteTaskQueue,
                                        IStatsDClient statsDClient)
        {
            this.eventFeedFactory = eventFeedFactory;
            GlobalTime = remoteTaskQueue.GlobalTime;
            globalTimeProvider = new RtqGlobalTimeProvider(GlobalTime);
            eventLogRepository = remoteTaskQueue.EventLogRepository;
            var graphiteReporter = new RtqElasticsearchIndexerGraphiteReporter("SubSystem.RemoteTaskQueue.ElasticsearchIndexer", statsDClient);
            var taskMetaProcessor = new TaskMetaProcessor(indexerSettings, elasticsearchClientFactory, remoteTaskQueue, graphiteReporter);
            eventConsumer = new RtqMonitoringEventConsumer(indexerSettings, taskMetaProcessor);
            offsetInterpreter = new RtqMonitoringOffsetInterpreter();
            this.elasticsearchClientFactory = elasticsearchClientFactory;
        }

        [NotNull]
        public IGlobalTime GlobalTime { get; }

        [NotNull]
        public IEventFeedsRunner RunEventFeeding()
        {
            const string key = "RtqMonitoring";
            return eventFeedFactory
                .WithOffsetType<string>()
                .WithEventType(BladesBuilder.New(eventLogRepository, eventConsumer)
                                            .WithBlade($"{key}_Blade0", delay : TimeSpan.FromMinutes(1))
                                            .WithBlade($"{key}_Blade1", delay : TimeSpan.FromMinutes(15)))
                .WithGlobalTimeProvider(globalTimeProvider)
                .WithOffsetInterpreter(offsetInterpreter)
                .WithOffsetStorageFactory(bladeId => new RtqElasticsearchOffsetStorage(elasticsearchClientFactory, offsetInterpreter, bladeId.BladeKey))
                .RunFeeds(delayBetweenIterations : TimeSpan.FromMinutes(1));
        }

        private readonly EventFeedFactory eventFeedFactory;
        private readonly RtqGlobalTimeProvider globalTimeProvider;
        private readonly EventLogRepository eventLogRepository;
        private readonly RtqMonitoringEventConsumer eventConsumer;
        private readonly RtqMonitoringOffsetInterpreter offsetInterpreter;
        private readonly RtqElasticsearchClientFactory elasticsearchClientFactory;
    }
}