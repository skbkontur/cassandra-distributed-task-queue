using System;

using RemoteTaskQueue.FunctionalTests.Common;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Indexer;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TaskCounter;

using SKBKontur.Catalogue.ServiceLib;
using SKBKontur.Catalogue.ServiceLib.HttpHandlers;
using SKBKontur.Catalogue.ServiceLib.Services;

using SkbKontur.Graphite.Client;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TestService
{
    public class MonitoringServiceEntryPoint : ApplicationBase
    {
        protected override string ConfigFileName => "monitoringService.csf";
        protected override string SubProject => null;

        private static void Main()
        {
            new MonitoringServiceEntryPoint().Run();
        }

        private void Run()
        {
            Container.ConfigureCassandra();

            Container.Configurator.ForAbstraction<IRtqElasticsearchClient>().UseInstances(new RtqElasticsearchClient(new Uri("http://localhost:9205")));
            Container.Configurator.ForAbstraction<IRtqTaskCounterStateStorage>().UseType<NoOpRtqTaskCounterStateStorage>();
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
            Container.Configurator.ForAbstraction<RtqTaskCounterSettings>().UseInstances(rtqTaskCounterSettings);
            var rtqElasticsearchIndexerSettings = new RtqElasticsearchIndexerSettings(eventFeedKey : "RtqElasticsearchIndexerEventFeed", rtqGraphitePathPrefix : "None");
            Container.Configurator.ForAbstraction<RtqElasticsearchIndexerSettings>().UseInstances(rtqElasticsearchIndexerSettings);
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
    }
}