using Elasticsearch.Net;

using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using RemoteTaskQueue.Monitoring.Storage;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.ServiceLib.HttpHandlers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TestService
{
    public class MonitoringServiceHttpHandler : IHttpHandler
    {
        public MonitoringServiceHttpHandler(IGlobalTime globalTime,
                                            RtqElasticsearchSchema rtqElasticsearchSchema,
                                            RtqElasticsearchClientFactory elasticsearchClientFactory)
        {
            this.globalTime = globalTime;
            this.rtqElasticsearchSchema = rtqElasticsearchSchema;
            this.elasticsearchClientFactory = elasticsearchClientFactory;
        }

        [HttpMethod]
        public void Stop()
        {
            //schedulableRunner.Stop();
        }

        [HttpMethod]
        public void UpdateAndFlush()
        {
            //indexer.ProcessNewEvents();
            elasticsearchClientFactory.DefaultClient.Value.IndicesRefresh("_all");
        }

        [HttpMethod]
        public void ResetState()
        {
            //schedulableRunner.Stop();

            DeleteAllElasticEntities();
            rtqElasticsearchSchema.Actualize(local : true, bulkLoad : false);

            globalTime.ResetInMemoryState();
            //indexerProgressMarkerStorage.SetIndexingStartTimestamp(new Timestamp(globalTime.GetNowTicks()));

            //schedulableRunner.Start();
        }

        private void DeleteAllElasticEntities()
        {
            var elasticsearchClient = elasticsearchClientFactory.DefaultClient.Value;
            elasticsearchClient.IndicesDelete(RtqElasticsearchConsts.IndexPrefix + "*").ProcessResponse(200, 404);
            elasticsearchClient.IndicesDeleteTemplateForAll(RtqElasticsearchConsts.TemplateName).ProcessResponse(200, 404);
            //TODO delete aliases
            elasticsearchClient.ClusterHealth(p => p.WaitForStatus(WaitForStatus.Green)).ProcessResponse();
        }

        private readonly IGlobalTime globalTime;
        private readonly RtqElasticsearchSchema rtqElasticsearchSchema;
        private readonly RtqElasticsearchClientFactory elasticsearchClientFactory;
    }
}