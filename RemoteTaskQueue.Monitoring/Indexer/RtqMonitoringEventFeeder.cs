using System;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;

using RemoteTaskQueue.Monitoring.Storage;
using RemoteTaskQueue.Monitoring.Storage.Writing;

using SKBKontur.Catalogue.Core.EventFeeds;
using SKBKontur.Catalogue.Core.EventFeeds.Firing;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class RtqMonitoringEventFeeder
    {
        public RtqMonitoringEventFeeder(MultiRazorEventFeedFactory eventFeedFactory,
                                        RtqGlobalTimeProvider globalTimeProvider,
                                        EventLogRepository eventLogRepository,
                                        RtqMonitoringEventConsumer eventConsumer,
                                        RtqMonitoringOffsetInterpreter offsetInterpreter,
                                        RtqElasticsearchClientFactory elasticsearchClientFactory)
        {
            this.eventFeedFactory = eventFeedFactory;
            this.globalTimeProvider = globalTimeProvider;
            this.eventLogRepository = eventLogRepository;
            this.eventConsumer = eventConsumer;
            this.offsetInterpreter = offsetInterpreter;
            this.elasticsearchClientFactory = elasticsearchClientFactory;
        }

        [NotNull]
        public IEventFeedsRunner RunEventFeeding()
        {
            const string key = "RtqMonitoring";
            return eventFeedFactory
                .Feed<TaskMetaUpdatedEvent, string>(key)
                .WithBlade(key + "_Blade0", delay : TimeSpan.FromMinutes(1))
                .WithBlade(key + "_Blade1", delay : TimeSpan.FromMinutes(15))
                .WithGlobalTimeProvider(globalTimeProvider)
                .WithEventSource(eventLogRepository)
                .WithConsumer(eventConsumer)
                .WithOffsetInterpreter(offsetInterpreter)
                .WithOffsetStorageFactory(bladeId => new RtqElasticsearchOffsetStorage(elasticsearchClientFactory, offsetInterpreter, bladeId.Key))
                .InParallel()
                .RunFeeds(delayBetweenIterations : TimeSpan.FromMinutes(1));
        }

        private readonly MultiRazorEventFeedFactory eventFeedFactory;
        private readonly RtqGlobalTimeProvider globalTimeProvider;
        private readonly EventLogRepository eventLogRepository;
        private readonly RtqMonitoringEventConsumer eventConsumer;
        private readonly RtqMonitoringOffsetInterpreter offsetInterpreter;
        private readonly RtqElasticsearchClientFactory elasticsearchClientFactory;
    }
}