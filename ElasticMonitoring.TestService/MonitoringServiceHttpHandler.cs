using System;

using Elasticsearch.Net;

using RemoteTaskQueue.Monitoring.Indexer;
using RemoteTaskQueue.Monitoring.Storage;
using RemoteTaskQueue.Monitoring.TaskCounter;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.Core.EventFeeds;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.ServiceLib.HttpHandlers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TestService
{
    public class MonitoringServiceHttpHandler : IHttpHandler
    {
        public MonitoringServiceHttpHandler(RtqTaskCounterEventFeeder taskCounterEventFeeder,
                                            RtqMonitoringEventFeeder monitoringEventFeeder,
                                            RtqElasticsearchSchema rtqElasticsearchSchema,
                                            RtqElasticsearchClientFactory elasticsearchClientFactory)
        {
            this.taskCounterEventFeeder = taskCounterEventFeeder;
            this.monitoringEventFeeder = monitoringEventFeeder;
            this.rtqElasticsearchSchema = rtqElasticsearchSchema;
            this.elasticsearchClientFactory = elasticsearchClientFactory;
        }

        [HttpMethod]
        public RtqTaskCounters GetTaskCounters()
        {
            return taskCounterStateManager.GetTaskCounters(Timestamp.Now);
        }

        [HttpMethod]
        public void Stop()
        {
            StopFeeding();
        }

        [HttpMethod]
        public void ExecuteForcedFeeding()
        {
            monitoringFeedsRunner.ResetLocalState();
            monitoringFeedsRunner.ExecuteForcedFeeding(delayUpperBound : TimeSpan.MaxValue);
            elasticsearchClientFactory.DefaultClient.Value.IndicesRefresh("_all");

            taskCounterFeedsRunner.ExecuteForcedFeeding(delayUpperBound : TimeSpan.MaxValue);
        }

        [HttpMethod]
        public void ResetState()
        {
            StopFeeding();

            DeleteAllElasticEntities();
            rtqElasticsearchSchema.Actualize(local : true, bulkLoad : false);
            monitoringEventFeeder.GlobalTime.ResetInMemoryState();
            taskCounterEventFeeder.GlobalTime.ResetInMemoryState();

            monitoringFeedsRunner = monitoringEventFeeder.RunEventFeeding();
            (taskCounterFeedsRunner, taskCounterStateManager) = taskCounterEventFeeder.RunEventFeeding();
        }

        private void StopFeeding()
        {
            StopFeeding(ref taskCounterFeedsRunner);
            StopFeeding(ref monitoringFeedsRunner);
        }

        private static void StopFeeding(ref IEventFeedsRunner feedsRunner)
        {
            if (feedsRunner != null)
            {
                feedsRunner.Stop();
                feedsRunner = null;
            }
        }

        private void DeleteAllElasticEntities()
        {
            var elasticsearchClient = elasticsearchClientFactory.DefaultClient.Value;
            elasticsearchClient.IndicesDelete(RtqElasticsearchConsts.AllIndicesWildcard).ProcessResponse(200, 404);
            elasticsearchClient.IndicesDeleteTemplateForAll(RtqElasticsearchConsts.TemplateName).ProcessResponse(200, 404);
            elasticsearchClient.ClusterHealth(p => p.WaitForStatus(WaitForStatus.Green)).ProcessResponse();
        }

        private IEventFeedsRunner monitoringFeedsRunner;
        private IEventFeedsRunner taskCounterFeedsRunner;
        private RtqTaskCounterStateManager taskCounterStateManager;
        private readonly RtqTaskCounterEventFeeder taskCounterEventFeeder;
        private readonly RtqMonitoringEventFeeder monitoringEventFeeder;
        private readonly RtqElasticsearchSchema rtqElasticsearchSchema;
        private readonly RtqElasticsearchClientFactory elasticsearchClientFactory;
    }
}