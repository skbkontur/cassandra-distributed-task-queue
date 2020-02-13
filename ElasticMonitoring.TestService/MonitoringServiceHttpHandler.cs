using System;

using Elasticsearch.Net;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Indexer;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TaskCounter;
using SkbKontur.Cassandra.TimeBasedUuid;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.Core.EventFeeds;
using SKBKontur.Catalogue.ServiceLib.HttpHandlers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TestService
{
    public class MonitoringServiceHttpHandler : IHttpHandler
    {
        public MonitoringServiceHttpHandler(RtqTaskCounterEventFeeder taskCounterEventFeeder,
                                            RtqMonitoringEventFeeder monitoringEventFeeder,
                                            RtqElasticsearchSchema rtqElasticsearchSchema,
                                            IRtqElasticsearchClient elasticClient)
        {
            this.taskCounterEventFeeder = taskCounterEventFeeder;
            this.monitoringEventFeeder = monitoringEventFeeder;
            this.rtqElasticsearchSchema = rtqElasticsearchSchema;
            this.elasticClient = elasticClient;
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
            elasticClient.IndicesRefresh<StringResponse>("_all").EnsureSuccess();

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
            (taskCounterFeedsRunner, taskCounterStateManager, _) = taskCounterEventFeeder.RunEventFeeding();
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
            elasticClient.IndicesDelete<StringResponse>(RtqElasticsearchConsts.AllIndicesWildcard, new DeleteIndexRequestParameters {RequestConfiguration = allowNotFoundStatusCode}).EnsureSuccess();
            elasticClient.IndicesDeleteTemplateForAll<StringResponse>(RtqElasticsearchConsts.TemplateName, new DeleteIndexTemplateRequestParameters {RequestConfiguration = allowNotFoundStatusCode}).EnsureSuccess();
            elasticClient.ClusterHealth<StringResponse>(new ClusterHealthRequestParameters {WaitForStatus = WaitForStatus.Green}).EnsureSuccess();
        }

        private IEventFeedsRunner monitoringFeedsRunner;
        private IEventFeedsRunner taskCounterFeedsRunner;
        private RtqTaskCounterStateManager taskCounterStateManager;
        private readonly RtqTaskCounterEventFeeder taskCounterEventFeeder;
        private readonly RtqMonitoringEventFeeder monitoringEventFeeder;
        private readonly RtqElasticsearchSchema rtqElasticsearchSchema;
        private readonly IRtqElasticsearchClient elasticClient;
        private readonly RequestConfiguration allowNotFoundStatusCode = new RequestConfiguration {AllowedStatusCodes = new[] {404}};
    }
}