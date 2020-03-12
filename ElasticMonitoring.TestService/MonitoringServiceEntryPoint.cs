using System;

using GroboContainer.Core;

using JetBrains.Annotations;

using RemoteTaskQueue.FunctionalTests.Common;

using SkbKontur.Cassandra.DistributedLock;
using SkbKontur.Cassandra.DistributedLock.RemoteLocker;
using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Indexer;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TaskCounter;

using SKBKontur.Catalogue.ServiceLib;
using SKBKontur.Catalogue.ServiceLib.HttpHandlers;
using SKBKontur.Catalogue.ServiceLib.Services;
using SKBKontur.Catalogue.TestCore.NUnit.Extensions;

using SkbKontur.Graphite.Client;

using Vostok.Logging.Abstractions;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TestService
{
    public class MonitoringServiceEntryPoint : ApplicationBase
    {
        protected override string ConfigFileName => "monitoringService.csf";

        private static void Main()
        {
            new MonitoringServiceEntryPoint().Run();
        }

        private void Run()
        {
            WithCassandra.SetUpCassandra(Container, TestRtqSettings.QueueKeyspaceName);

            ConfigureRemoteLock(Container);
            Container.Configurator.ForAbstraction<IRtqElasticsearchClient>().UseInstances(new RtqElasticsearchClient(new Uri("http://localhost:9205")));
            Container.Configurator.ForAbstraction<IRtqTaskCounterStateStorage>().UseType<NoOpRtqTaskCounterStateStorage>();
            Container.Configurator.ForAbstraction<RtqTaskCounterSettings>().UseInstances(new RtqTaskCounterSettings
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
                });
            Container.ConfigureRemoteTaskQueue(out var remoteTaskQueue);

            var rtqMonitoringEventFeeder = Container.Create<SkbKontur.Cassandra.DistributedTaskQueue.Handling.RemoteTaskQueue, IStatsDClient, RtqMonitoringEventFeeder>(remoteTaskQueue, NoOpStatsDClient.Instance);
            Container.Configurator.ForAbstraction<IRtqMonitoringEventFeeder>().UseInstances(rtqMonitoringEventFeeder);

            var rtqTaskCounterEventFeeder = Container.Create<SkbKontur.Cassandra.DistributedTaskQueue.Handling.RemoteTaskQueue, IStatsDClient, RtqTaskCounterEventFeeder>(remoteTaskQueue, NoOpStatsDClient.Instance);
            Container.Configurator.ForAbstraction<IRtqTaskCounterEventFeeder>().UseInstances(rtqTaskCounterEventFeeder);

            Container.Get<ElasticAvailabilityChecker>().WaitAlive();
            Container.Get<RtqElasticsearchSchema>().Actualize(local : true, bulkLoad : false);
            Container.Configurator.ForAbstraction<IHttpHandler>().UseType<MonitoringServiceHttpHandler>();
            Container.Get<HttpService>().Run();
        }

        private static void ConfigureRemoteLock([NotNull] IContainer container)
        {
            var logger = container.Get<ILog>();
            const string locksKeyspace = TestRtqSettings.QueueKeyspaceName;
            const string locksColumnFamily = RtqColumnFamilyRegistry.LocksColumnFamilyName;
            var remoteLockImplementationSettings = CassandraRemoteLockImplementationSettings.Default(locksKeyspace, locksColumnFamily);
            var remoteLockImplementation = container.Create<CassandraRemoteLockImplementationSettings, CassandraRemoteLockImplementation>(remoteLockImplementationSettings);
            var remoteLockCreator = new RemoteLocker(remoteLockImplementation, new RemoteLockerMetrics($"{locksKeyspace}_{locksColumnFamily}"), logger);
            container.Configurator.ForAbstraction<IRemoteLockCreator>().UseInstances(remoteLockCreator);
        }
    }
}