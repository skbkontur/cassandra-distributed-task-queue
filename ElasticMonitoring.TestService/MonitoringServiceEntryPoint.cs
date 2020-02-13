using System;

using RemoteTaskQueue.FunctionalTests.Common;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Indexer;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TaskCounter;

using SKBKontur.Catalogue.ServiceLib;
using SKBKontur.Catalogue.ServiceLib.Services;

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
    }
}