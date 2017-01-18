using System;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.Core.EventFeeds;
using SKBKontur.Catalogue.Core.EventFeeds.OffsetStorages;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class RtqMonitoringEventConsumer : IEventConsumer<TaskMetaUpdatedEvent>
    {
        [NotNull]
        public string GetDescription()
        {
            return GetType().FullName;
        }

        public void Initialize()
        {
        }

        public void Shutdown()
        {
        }

        public void ProcessEvents([NotNull] IObjectMutationEvent<TaskMetaUpdatedEvent>[] modificationEvents)
        {
            throw new NotImplementedException();
        }
    }

    public class RtqMonitoringEventSource : IEventSource<TaskMetaUpdatedEvent>
    {
        [NotNull]
        public string GetDescription()
        {
            return GetType().FullName;
        }

        [NotNull]
        public EventsQueryResult<TaskMetaUpdatedEvent, long> GetEvents(long fromOffsetExclusive, long toOffsetInclusive, int estimatedCount)
        {
            return new EventsQueryResult<TaskMetaUpdatedEvent, long>();
        }
    }

    public class RtqMonitoringEventFeeder
    {
        public RtqMonitoringEventFeeder(MultiRazorEventFeedFactory eventFeedFactory, 
                                        IEventSource<TaskMetaUpdatedEvent> eventSource, 
                                        IEventConsumer<TaskMetaUpdatedEvent> eventConsumer,
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
                .Feed<TaskMetaUpdatedEvent, long>(key)
                .WithEventSource(eventSource)
                .WithConsumer(eventConsumer)
                .WithOffsetStorageFactory(bladeId => elasticsearchOffsetStorageProvider
                                                         .ClientDataStadardOffsetStorage<long>(bladeId.Key)
                                                         .AndRollbackIfOffsetEmpty(TimeSpan.FromHours(1).Ticks))
                .WithBlade(key + "_Blade0", TimeSpan.FromMinutes(1))
                .WithBlade(key + "_Blade1", TimeSpan.FromMinutes(15))
                .WithLeaderElection()
                .InParallel()
                .FirePeriodicTasks(TimeSpan.FromMinutes(1));
        }

        private readonly MultiRazorEventFeedFactory eventFeedFactory;
        private readonly IEventSource<TaskMetaUpdatedEvent> eventSource;
        private readonly IEventConsumer<TaskMetaUpdatedEvent> eventConsumer;
        private readonly ElasticsearchOffsetStorageProvider elasticsearchOffsetStorageProvider;
    }
}