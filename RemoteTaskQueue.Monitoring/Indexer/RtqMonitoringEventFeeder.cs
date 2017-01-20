using System;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

using RemoteTaskQueue.Monitoring.Storage;
using RemoteTaskQueue.Monitoring.Storage.Writing;

using SKBKontur.Catalogue.Core.EventFeeds;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class RtqMonitoringEventConsumer : IEventConsumer<TaskMetaUpdatedEvent, string>
    {
        [NotNull]
        public string GetDescription()
        {
            return GetType().FullName;
        }

        [NotNull]
        public EventsProcessingResult<string> ProcessEvents([NotNull] EventsQueryResult<TaskMetaUpdatedEvent, string> eventsQueryResult)
        {
            throw new NotImplementedException();
        }
    }

    public class RtqMonitoringEventSource : IEventSource<TaskMetaUpdatedEvent, string>
    {
        [NotNull]
        public string GetDescription()
        {
            return GetType().FullName;
        }

        [NotNull]
        public EventsQueryResult<TaskMetaUpdatedEvent, string> GetEvents(string fromOffsetExclusive, string toOffsetInclusive, int estimatedCount)
        {
            throw new NotImplementedException();
        }
    }

    public class RtqMonitoringEventFeeder
    {
        public RtqMonitoringEventFeeder(MultiRazorEventFeedFactory eventFeedFactory,
                                        IEventSource<TaskMetaUpdatedEvent, string> eventSource,
                                        IEventConsumer<TaskMetaUpdatedEvent, string> eventConsumer,
                                        RtqElasticsearchClientFactory elasticsearchClientFactory)
        {
            this.eventFeedFactory = eventFeedFactory;
            this.eventSource = eventSource;
            this.eventConsumer = eventConsumer;
            this.elasticsearchClientFactory = elasticsearchClientFactory;
        }

        private void RunEventFeeding()
        {
            const string key = "RtqMonitoring";
            eventFeedFactory
                .Feed<TaskMetaUpdatedEvent, string>(key)
                .WithBlade(key + "_Blade0", TimeSpan.FromMinutes(1))
                .WithBlade(key + "_Blade1", TimeSpan.FromMinutes(15))
                .WithEventSource(eventSource)
                .WithConsumer(eventConsumer)
                .WithOffsetStorageFactory(bladeId => new RtqElasticsearchOffsetStorage(elasticsearchClientFactory, bladeId.Key))
                .InParallel()
                .RunFeeds(TimeSpan.FromMinutes(1));
        }

        private readonly MultiRazorEventFeedFactory eventFeedFactory;
        private readonly IEventSource<TaskMetaUpdatedEvent, string> eventSource;
        private readonly IEventConsumer<TaskMetaUpdatedEvent, string> eventConsumer;
        private readonly RtqElasticsearchClientFactory elasticsearchClientFactory;
    }
}