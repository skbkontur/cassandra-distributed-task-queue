using Elasticsearch.Net;

using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using RemoteTaskQueue.Monitoring.Indexer;
using RemoteTaskQueue.Monitoring.Storage;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.ServiceLib.HttpHandlers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TestService
{
    public class MonitoringServiceHttpHandler : IHttpHandler
    {
        public MonitoringServiceHttpHandler(ITaskIndexController taskIndexController,
                                            IGlobalTime globalTime,
                                            RtqElasticsearchSchema rtqElasticsearchSchema,
                                            MonitoringServiceSchedulableRunner schedulableRunner,
                                            RtqElasticsearchClientFactory elasticsearchClientFactory)
        {
            this.taskIndexController = taskIndexController;
            this.globalTime = globalTime;
            this.rtqElasticsearchSchema = rtqElasticsearchSchema;
            this.schedulableRunner = schedulableRunner;
            this.elasticsearchClientFactory = elasticsearchClientFactory;
        }

        [HttpMethod]
        public void UpdateAndFlush()
        {
            taskIndexController.ProcessNewEvents();
            elasticsearchClientFactory.DefaultClient.Value.IndicesRefresh("_all");
        }

        [HttpMethod]
        public void ResetState()
        {
            schedulableRunner.Stop();

            taskIndexController.SetMinTicksHack(globalTime.GetNowTicks());
            globalTime.ResetInMemoryState();

            DeleteAllElasticEntities();
            rtqElasticsearchSchema.ActualizeTemplate(local : true);

            schedulableRunner.Start();
        }

        private void DeleteAllElasticEntities()
        {
            var elasticsearchClient = elasticsearchClientFactory.DefaultClient.Value;
            elasticsearchClient.ClearScroll("_all").ProcessResponse(); //todo плохо, мешает чужим поискам
            elasticsearchClient.IndicesDelete(RtqElasticsearchConsts.IndexPrefix + "*").ProcessResponse(200, 404);
            elasticsearchClient.IndicesDeleteTemplateForAll(RtqElasticsearchConsts.TemplateName).ProcessResponse(200, 404);
            //TODO delete aliases
            elasticsearchClient.ClusterHealth(p => p.WaitForStatus(WaitForStatus.Green)).ProcessResponse();
        }

        private readonly ITaskIndexController taskIndexController;
        private readonly IGlobalTime globalTime;
        private readonly RtqElasticsearchSchema rtqElasticsearchSchema;
        private readonly MonitoringServiceSchedulableRunner schedulableRunner;
        private readonly RtqElasticsearchClientFactory elasticsearchClientFactory;
    }
}