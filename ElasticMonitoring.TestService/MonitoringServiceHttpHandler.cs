using System;

using Elasticsearch.Net;

using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using RemoteTaskQueue.Monitoring.Indexer;
using RemoteTaskQueue.Monitoring.Storage;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.Core.EventFeeds;
using SKBKontur.Catalogue.ServiceLib.HttpHandlers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TestService
{
    public class MonitoringServiceHttpHandler : IHttpHandler
    {
        public MonitoringServiceHttpHandler(IGlobalTime globalTime,
                                            RtqMonitoringEventFeeder eventFeeder,
                                            RtqElasticsearchSchema rtqElasticsearchSchema,
                                            RtqElasticsearchClientFactory elasticsearchClientFactory)
        {
            this.globalTime = globalTime;
            this.eventFeeder = eventFeeder;
            this.rtqElasticsearchSchema = rtqElasticsearchSchema;
            this.elasticsearchClientFactory = elasticsearchClientFactory;
        }

        [HttpMethod]
        public void Stop()
        {
            StopFeeding();
        }

        [HttpMethod]
        public void ExecuteForcedFeeding()
        {
            foreach(var feed in feedsRunner.RunningFeeds)
            {
                feed.ResetLocalState();
                feed.ExecuteForcedFeeding(delayUpperBound : TimeSpan.MaxValue);
            }
            elasticsearchClientFactory.DefaultClient.Value.IndicesRefresh("_all");
        }

        [HttpMethod]
        public void ResetState()
        {
            StopFeeding();

            DeleteAllElasticEntities();
            rtqElasticsearchSchema.Actualize(local : true, bulkLoad : false);
            globalTime.ResetInMemoryState();

            feedsRunner = eventFeeder.RunEventFeeding();
        }

        private void StopFeeding()
        {
            if(feedsRunner != null)
            {
                feedsRunner.Stop();
                feedsRunner = null;
            }
        }

        private void DeleteAllElasticEntities()
        {
            var elasticsearchClient = elasticsearchClientFactory.DefaultClient.Value;
            elasticsearchClient.IndicesDelete(RtqElasticsearchConsts.IndexPrefix + "*").ProcessResponse(200, 404);
            elasticsearchClient.IndicesDeleteTemplateForAll(RtqElasticsearchConsts.TemplateName).ProcessResponse(200, 404);
            elasticsearchClient.ClusterHealth(p => p.WaitForStatus(WaitForStatus.Green)).ProcessResponse();
        }

        private IEventFeedsRunner feedsRunner;
        private readonly IGlobalTime globalTime;
        private readonly RtqMonitoringEventFeeder eventFeeder;
        private readonly RtqElasticsearchSchema rtqElasticsearchSchema;
        private readonly RtqElasticsearchClientFactory elasticsearchClientFactory;
    }
}