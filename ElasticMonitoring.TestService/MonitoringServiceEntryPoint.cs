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
using SkbKontur.Cassandra.DistributedTaskQueue.Settings;

using SKBKontur.Catalogue.ServiceLib;
using SKBKontur.Catalogue.ServiceLib.Services;

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
            Container.ConfigureForTestRemoteTaskQueue();
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
            Container.Get<ElasticAvailabilityChecker>().WaitAlive();
            Container.Get<RtqElasticsearchSchema>().Actualize(local : true, bulkLoad : false);
            Container.Get<HttpService>().Run();
        }

        private static void ConfigureRemoteLock([NotNull] IContainer container)
        {
            var logger = container.Get<ILog>();
            var locksKeyspace = container.Get<IRtqSettings>().NewQueueKeyspace;
            const string locksColumnFamily = RtqColumnFamilyRegistry.LocksColumnFamilyName;
            var remoteLockImplementationSettings = CassandraRemoteLockImplementationSettings.Default(locksKeyspace, locksColumnFamily);
            var remoteLockImplementation = container.Create<CassandraRemoteLockImplementationSettings, CassandraRemoteLockImplementation>(remoteLockImplementationSettings);
            var remoteLockCreator = new RemoteLocker(remoteLockImplementation, new RemoteLockerMetrics($"{locksKeyspace}_{locksColumnFamily}"), logger);
            container.Configurator.ForAbstraction<IRemoteLockCreator>().UseInstances(remoteLockCreator);
        }
    }
}