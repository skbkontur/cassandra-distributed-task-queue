using Elasticsearch.Net;

using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Actualizer;
using SKBKontur.Catalogue.ServiceLib.HttpHandlers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TestService
{
    public class ElasticMonitoringTestHelpersHttpHandler : IHttpHandler
    {
        public ElasticMonitoringTestHelpersHttpHandler(ITaskIndexController taskIndexController,
                                                       IGlobalTime globalTime,
                                                       TaskSearchIndexSchema taskSearchIndexSchema,
                                                       ElasticMonitoringServiceSchedulableRunner schedulableRunner,
                                                       RtqElasticsearchClientFactory elasticsearchClientFactory)
        {
            this.taskIndexController = taskIndexController;
            this.globalTime = globalTime;
            this.taskSearchIndexSchema = taskSearchIndexSchema;
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
            taskSearchIndexSchema.ActualizeTemplate(local : true);

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
        private readonly TaskSearchIndexSchema taskSearchIndexSchema;
        private readonly ElasticMonitoringServiceSchedulableRunner schedulableRunner;
        private readonly RtqElasticsearchClientFactory elasticsearchClientFactory;
    }
}