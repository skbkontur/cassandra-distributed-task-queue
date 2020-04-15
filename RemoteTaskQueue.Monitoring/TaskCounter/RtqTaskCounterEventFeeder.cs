using GroBuf;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories;
using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.GlobalTimestamp;

using SKBKontur.Catalogue.Core.EventFeeds;
using SKBKontur.Catalogue.Objects.Json;
using SKBKontur.Catalogue.ServiceLib.Scheduling;

using SkbKontur.EventFeeds;
using SkbKontur.Graphite.Client;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TaskCounter
{
    [PublicAPI]
    public class RtqTaskCounterEventFeeder : IRtqTaskCounterEventFeeder
    {
        public RtqTaskCounterEventFeeder(ILog logger,
                                         ISerializer serializer,
                                         RtqTaskCounterSettings settings,
                                         IRtqTaskDataRegistry taskDataRegistry,
                                         IRtqTaskCounterStateStorage stateStorage,
                                         IGraphiteClient graphiteClient,
                                         IStatsDClient statsDClient,
                                         IPeriodicTaskRunner periodicTaskRunner,
                                         IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection,
                                         RemoteTaskQueue remoteTaskQueue)
        {
            this.serializer = serializer;
            this.settings = settings;
            this.taskDataRegistry = taskDataRegistry;
            this.stateStorage = stateStorage;
            GlobalTime = remoteTaskQueue.GlobalTime;
            var graphiteLagReporter = new EventFeedsGraphiteLagReporter(graphiteClient, periodicTaskRunner);
            var eventFeedPeriodicJobRunner = new EventFeedPeriodicJobRunner(periodicJobRunnerWithLeaderElection, graphiteLagReporter);
            eventFeedFactory = new EventFeedFactory(new EventFeedGlobalTimeProvider(GlobalTime), eventFeedPeriodicJobRunner);
            eventLogRepository = remoteTaskQueue.EventLogRepository;
            handleTasksMetaStorage = remoteTaskQueue.HandleTasksMetaStorage;
            perfGraphiteReporter = new RtqMonitoringPerfGraphiteReporter(settings.PerfGraphitePrefix, statsDClient);
            this.logger = logger.ForContext("CassandraDistributedTaskQueue").ForContext(nameof(RtqTaskCounterEventFeeder));
            this.logger.Info($"Using RtqTaskCounterSettings: {settings.ToPrettyJson()}");
        }

        [NotNull]
        public IGlobalTime GlobalTime { get; }

        public ( /*[NotNull]*/ IEventFeedsRunner, /*[NotNull]*/ RtqTaskCounterStateManager) RunEventFeeding()
        {
            var stateManager = new RtqTaskCounterStateManager(logger, serializer, taskDataRegistry, stateStorage, settings, offsetInterpreter, perfGraphiteReporter);
            var eventConsumer = new RtqTaskCounterEventConsumer(stateManager, handleTasksMetaStorage, perfGraphiteReporter);
            IBladesBuilder<string> bladesBuilder = BladesBuilder.New(eventLogRepository, eventConsumer, logger);
            foreach (var bladeId in stateManager.Blades)
                bladesBuilder = bladesBuilder.WithBlade(bladeId.BladeKey, bladeId.Delay);
            var eventFeedsRunner = eventFeedFactory
                                   .WithOffsetType<string>()
                                   .WithEventType(bladesBuilder)
                                   .WithOffsetInterpreter(offsetInterpreter)
                                   .WithOffsetStorageFactory(bladeId => stateManager.CreateOffsetStorage(bladeId))
                                   .WithSingleLeaderElectionKey(stateManager.CompositeFeedKey)
                                   .RunFeeds(settings.DelayBetweenEventFeedingIterations);
            return (eventFeedsRunner, stateManager);
        }

        private readonly ILog logger;
        private readonly ISerializer serializer;
        private readonly RtqTaskCounterSettings settings;
        private readonly IRtqTaskDataRegistry taskDataRegistry;
        private readonly IRtqTaskCounterStateStorage stateStorage;
        private readonly EventFeedFactory eventFeedFactory;
        private readonly EventLogRepository eventLogRepository;
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly RtqMonitoringPerfGraphiteReporter perfGraphiteReporter;
        private readonly RtqEventLogOffsetInterpreter offsetInterpreter = new RtqEventLogOffsetInterpreter();
    }
}