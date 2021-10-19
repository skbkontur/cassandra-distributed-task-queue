using System;

using GroboContainer.Core;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

using SkbKontur.Cassandra.DistributedLock;
using SkbKontur.Cassandra.DistributedLock.RemoteLocker;
using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Indexer;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TaskCounter;
using SkbKontur.Graphite.Client;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TestService
{
    public class GroboControllerFactory : IControllerFactory
    {
        public object CreateController(ControllerContext controllerContext)
        {
            var controllerType = controllerContext.ActionDescriptor.ControllerTypeInfo.AsType();
            var controller = groboContainer.Create(controllerType);
            ((ControllerBase)controller).ControllerContext = controllerContext;
            return controller;
        }

        public void ReleaseController(ControllerContext context, object controller)
        {
        }

        private static IContainer ConfigureContainer()
        {
            var container = ApplicationBase.Initialize();
            container.ConfigureCassandra();
            ConfigureRemoteLock(container);
            var elasticSearchUrl = new Uri(Environment.GetEnvironmentVariable("ES_URL") ?? "http://localhost:9205");
            container.Configurator.ForAbstraction<IRtqElasticsearchClient>().UseInstances(new RtqElasticsearchClient(elasticSearchUrl));
            container.Configurator.ForAbstraction<IRtqTaskCounterStateStorage>().UseType<NoOpRtqTaskCounterStateStorage>();
            var rtqTaskCounterSettings = new RtqTaskCounterSettings(eventFeedKey : "RtqTaskCounterEventFeed", rtqGraphitePathPrefix : "None")
                {
                    BladeDelays = new[]
                        {
                            TimeSpan.Zero,
                            TimeSpan.FromSeconds(1),
                            TimeSpan.FromSeconds(5)
                        },
                    DelayBetweenEventFeedingIterations = TimeSpan.FromSeconds(1),
                    StatePersistingInterval = TimeSpan.FromSeconds(1),
                    PendingTaskExecutionUpperBound = TimeSpan.FromSeconds(5)
                };
            container.Configurator.ForAbstraction<RtqTaskCounterSettings>().UseInstances(rtqTaskCounterSettings);
            var rtqElasticsearchIndexerSettings = new RtqElasticsearchIndexerSettings(eventFeedKey : "RtqElasticsearchIndexerEventFeed", rtqGraphitePathPrefix : "None");
            container.Configurator.ForAbstraction<RtqElasticsearchIndexerSettings>().UseInstances(rtqElasticsearchIndexerSettings);
            container.ConfigureRemoteTaskQueue(out var remoteTaskQueue);

            var rtqMonitoringEventFeeder = container.Create<Handling.RemoteTaskQueue, IStatsDClient, RtqMonitoringEventFeeder>(remoteTaskQueue, NoOpStatsDClient.Instance);
            container.Configurator.ForAbstraction<IRtqMonitoringEventFeeder>().UseInstances(rtqMonitoringEventFeeder);

            var rtqTaskCounterEventFeeder = container.Create<Handling.RemoteTaskQueue, IStatsDClient, RtqTaskCounterEventFeeder>(remoteTaskQueue, NoOpStatsDClient.Instance);
            container.Configurator.ForAbstraction<IRtqTaskCounterEventFeeder>().UseInstances(rtqTaskCounterEventFeeder);

            container.Get<ElasticAvailabilityChecker>().WaitAlive();
            container.Get<RtqElasticsearchSchema>().Actualize(local : true, bulkLoad : false);

            var monitoringService = container.Create<MonitoringService>();
            container.Configurator.ForAbstraction<IMonitoringService>().UseInstances(monitoringService);

            return container;
        }

        private static void ConfigureRemoteLock(IContainer container)
        {
            var logger = container.Get<ILog>();
            const string locksKeyspace = TestRtqSettings.QueueKeyspaceName;
            const string locksColumnFamily = RtqColumnFamilyRegistry.LocksColumnFamilyName;
            var remoteLockImplementationSettings = CassandraRemoteLockImplementationSettings.Default(locksKeyspace, locksColumnFamily);
            var remoteLockImplementation = container.Create<CassandraRemoteLockImplementationSettings, CassandraRemoteLockImplementation>(remoteLockImplementationSettings);
            var remoteLockCreator = new RemoteLocker(remoteLockImplementation, new RemoteLockerMetrics($"{locksKeyspace}_{locksColumnFamily}"), logger);
            container.Configurator.ForAbstraction<IRemoteLockCreator>().UseInstances(remoteLockCreator);
        }

        private static readonly IContainer groboContainer = ConfigureContainer();
    }
}