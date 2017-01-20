using System;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.Core.EventFeeds;
using SKBKontur.Catalogue.Core.EventFeeds.OffsetStorages;

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
                                        ElasticsearchOffsetStorageProvider elasticsearchOffsetStorageProvider)
        {
            this.eventFeedFactory = eventFeedFactory;
            this.eventSource = eventSource;
            this.eventConsumer = eventConsumer;
            this.elasticsearchOffsetStorageProvider = elasticsearchOffsetStorageProvider;
        }

        private void RunEventFeeding()
        {
            const string key = "RtqMonitoring";
            eventFeedFactory
                .Feed<TaskMetaUpdatedEvent, string>(key)
                .WithEventSource(eventSource)
                .WithConsumer(eventConsumer)
                .WithOffsetStorageFactory(bladeId => elasticsearchOffsetStorageProvider
                                                         .ClientDataStadardOffsetStorage<string>(bladeId.Key))
                                                         //.AndRollbackIfOffsetEmpty(TimeSpan.FromHours(1).Ticks))
                .WithBlade(key + "_Blade0", TimeSpan.FromMinutes(1))
                .WithBlade(key + "_Blade1", TimeSpan.FromMinutes(15))
                .InParallel()
                .RunFeeds(TimeSpan.FromMinutes(1));
        }

        private readonly MultiRazorEventFeedFactory eventFeedFactory;
        private readonly IEventSource<TaskMetaUpdatedEvent, string> eventSource;
        private readonly IEventConsumer<TaskMetaUpdatedEvent, string> eventConsumer;
        private readonly ElasticsearchOffsetStorageProvider elasticsearchOffsetStorageProvider;
    }
}