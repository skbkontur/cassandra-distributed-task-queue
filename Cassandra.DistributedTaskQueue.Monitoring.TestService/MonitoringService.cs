using System;
using System.Threading;

using Elasticsearch.Net;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Indexer;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TaskCounter;
using SkbKontur.Cassandra.TimeBasedUuid;
using SkbKontur.EventFeeds;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TestService
{
    public class MonitoringService : IMonitoringService
    {
        public MonitoringService(IRtqTaskCounterEventFeeder taskCounterEventFeeder,
                                 IRtqMonitoringEventFeeder monitoringEventFeeder,
                                 RtqElasticsearchSchema rtqElasticsearchSchema,
                                 IRtqElasticsearchClient elasticClient)
        {
            this.taskCounterEventFeeder = (RtqTaskCounterEventFeeder)taskCounterEventFeeder;
            this.monitoringEventFeeder = (RtqMonitoringEventFeeder)monitoringEventFeeder;
            this.rtqElasticsearchSchema = rtqElasticsearchSchema;
            this.elasticClient = elasticClient;
        }

        public RtqTaskCounters GetTaskCounters()
        {
            return taskCounterStateManager.GetTaskCounters(Timestamp.Now);
        }

        public void Stop()
        {
            StopFeeding();
        }

        public void ExecuteForcedFeeding()
        {
            monitoringFeedsRunner.ResetLocalState();
            monitoringFeedsRunner.ExecuteForcedFeeding(delayUpperBound : TimeSpan.MaxValue);
            elasticClient.IndicesRefresh<StringResponse>("_all").EnsureSuccess();

            taskCounterFeedsRunner.ExecuteForcedFeeding(delayUpperBound : TimeSpan.MaxValue);
        }

        public void ResetState()
        {
            StopFeeding();

            DeleteAllElasticEntities();
            rtqElasticsearchSchema.Actualize(local : true, bulkLoad : false);
            monitoringEventFeeder.GlobalTime.ResetInMemoryState();
            taskCounterEventFeeder.GlobalTime.ResetInMemoryState();

            monitoringFeedsRunner = monitoringEventFeeder.RunEventFeeding(CancellationToken.None);
            (taskCounterFeedsRunner, taskCounterStateManager) = taskCounterEventFeeder.RunEventFeeding(CancellationToken.None);
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