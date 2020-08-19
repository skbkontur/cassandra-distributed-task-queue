using System;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.EventFeeds;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage.Writing;
using SkbKontur.Cassandra.DistributedTaskQueue.Scheduling;
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
                                        IPeriodicTaskRunner periodicTaskRunner,
                                        IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection,
                                        RemoteTaskQueue remoteTaskQueue)
        {
            this.logger = logger.ForContext("CassandraDistributedTaskQueue").ForContext(nameof(RtqMonitoringEventFeeder));
            this.indexerSettings = indexerSettings;
            this.elasticsearchClient = elasticsearchClient;
            GlobalTime = remoteTaskQueue.GlobalTime;
            eventLogRepository = remoteTaskQueue.EventLogRepository;
            var graphiteLagReporter = new EventFeedsGraphiteLagReporter(graphiteClient, periodicTaskRunner);
            var eventFeedPeriodicJobRunner = new EventFeedPeriodicJobRunner(periodicJobRunnerWithLeaderElection, graphiteLagReporter);
            eventFeedFactory = new EventFeedFactory(new EventFeedGlobalTimeProvider(GlobalTime), eventFeedPeriodicJobRunner);
            var perfGraphiteReporter = new RtqMonitoringPerfGraphiteReporter(indexerSettings.PerfGraphitePathPrefix, statsDClient);
            var taskMetaProcessor = new TaskMetaProcessor(this.logger, indexerSettings, elasticsearchClient, remoteTaskQueue, perfGraphiteReporter);
            eventConsumer = new RtqMonitoringEventConsumer(indexerSettings, taskMetaProcessor);
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
                   .WithOffsetInterpreter(offsetInterpreter)
                   .WithOffsetStorageFactory(bladeId => new RtqElasticsearchOffsetStorage(elasticsearchClient, offsetInterpreter, bladeId.BladeKey))
                   .RunFeeds(delayBetweenIterations : TimeSpan.FromMinutes(1));
        }

        private readonly ILog logger;
        private readonly RtqElasticsearchIndexerSettings indexerSettings;
        private readonly IRtqElasticsearchClient elasticsearchClient;
        private readonly EventLogRepository eventLogRepository;
        private readonly EventFeedFactory eventFeedFactory;
        private readonly RtqMonitoringEventConsumer eventConsumer;
        private readonly RtqEventLogOffsetInterpreter offsetInterpreter = new RtqEventLogOffsetInterpreter();
    }
}