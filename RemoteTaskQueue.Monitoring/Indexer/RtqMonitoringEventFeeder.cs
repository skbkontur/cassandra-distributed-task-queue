using System;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage.Writing;
using SkbKontur.Cassandra.GlobalTimestamp;

using SKBKontur.Catalogue.Core.EventFeeds;
using SKBKontur.Catalogue.ServiceLib.Scheduling;

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
            eventFeedFactory = new EventFeedFactory(new EventFeedGlobalTimeProvider(GlobalTime), new EventFeedPeriodicJobRunner(periodicJobRunnerWithLeaderElection));
            var perfGraphiteReporter = new RtqMonitoringPerfGraphiteReporter(indexerSettings.PerfGraphitePrefix, statsDClient);
            var taskMetaProcessor = new TaskMetaProcessor(this.logger, indexerSettings, elasticsearchClient, remoteTaskQueue, perfGraphiteReporter);
            eventConsumer = new RtqMonitoringEventConsumer(indexerSettings, taskMetaProcessor);
            offsetInterpreter = new RtqEventLogOffsetInterpreter();
            graphiteLagReporter = new EventFeedsGraphiteLagReporter(graphiteClient, periodicTaskRunner);
        }

        [NotNull]
        public IGlobalTime GlobalTime { get; }

        [NotNull]
        public IEventFeedsRunner RunEventFeeding()
        {
            var eventFeedsRunner = eventFeedFactory
                                   .WithOffsetType<string>()
                                   .WithEventType(BladesBuilder.New(eventLogRepository, eventConsumer, logger)
                                                               .WithBlade($"{indexerSettings.EventFeedKey}_Blade0", delay : TimeSpan.FromMinutes(1))
                                                               .WithBlade($"{indexerSettings.EventFeedKey}_Blade1", delay : TimeSpan.FromMinutes(15)))
                                   .WithOffsetInterpreter(offsetInterpreter)
                                   .WithOffsetStorageFactory(bladeId => new RtqElasticsearchOffsetStorage(elasticsearchClient, offsetInterpreter, bladeId.BladeKey))
                                   .RunFeeds(delayBetweenIterations : TimeSpan.FromMinutes(1));

            graphiteLagReporter.Start(eventFeedsRunner, eventFeedsLagReportingJobName : $"{indexerSettings.EventFeedKey}-ReportActualizationLagJob");

            return eventFeedsRunner;
        }

        private readonly ILog logger;
        private readonly RtqElasticsearchIndexerSettings indexerSettings;
        private readonly IRtqElasticsearchClient elasticsearchClient;
        private readonly EventLogRepository eventLogRepository;
        private readonly EventFeedFactory eventFeedFactory;
        private readonly RtqMonitoringEventConsumer eventConsumer;
        private readonly RtqEventLogOffsetInterpreter offsetInterpreter;
        private readonly EventFeedsGraphiteLagReporter graphiteLagReporter;
    }
}