using System;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;

using RemoteTaskQueue.Monitoring.Storage;
using RemoteTaskQueue.Monitoring.Storage.Writing;

using SKBKontur.Catalogue.Core.EventFeeds;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class RtqMonitoringEventFeeder
    {
        public RtqMonitoringEventFeeder(MultiRazorEventFeedFactory eventFeedFactory,
                                        EventLogRepository eventLogRepository,
                                        RtqMonitoringEventConsumer eventConsumer,
                                        RtqMonitoringOffsetInterpreter offsetInterpreter,
                                        RtqElasticsearchClientFactory elasticsearchClientFactory)
        {
            this.eventFeedFactory = eventFeedFactory;
            this.eventLogRepository = eventLogRepository;
            this.eventConsumer = eventConsumer;
            this.offsetInterpreter = offsetInterpreter;
            this.elasticsearchClientFactory = elasticsearchClientFactory;
        }

        public void RunEventFeeding()
        {
            const string key = "RtqMonitoring";
            eventFeedFactory
                .Feed<TaskMetaUpdatedEvent, string>(key)
                .WithBlade(key + "_Blade0", delay : TimeSpan.FromMinutes(1))
                .WithBlade(key + "_Blade1", delay : TimeSpan.FromMinutes(15))
                .WithEventSource(eventLogRepository)
                .WithConsumer(eventConsumer)
                .WithOffsetInterpreter(offsetInterpreter)
                .WithOffsetStorageFactory(bladeId => new RtqElasticsearchOffsetStorage(elasticsearchClientFactory, offsetInterpreter, bladeId.Key))
                .InParallel()
                .RunFeeds(delayBetweenIterations : TimeSpan.FromMinutes(1));
        }

        private readonly MultiRazorEventFeedFactory eventFeedFactory;
        private readonly EventLogRepository eventLogRepository;
        private readonly RtqMonitoringEventConsumer eventConsumer;
        private readonly RtqMonitoringOffsetInterpreter offsetInterpreter;
        private readonly RtqElasticsearchClientFactory elasticsearchClientFactory;
    }
}