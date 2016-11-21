using Elasticsearch.Net;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Actualizer
{
    public class TaskSearchIndexDataTestService
    {
        public TaskSearchIndexDataTestService(
            InternalDataElasticsearchFactory elasticsearchClientFactory,
            TaskSchemaDynamicSettings settings)
        {
            this.settings = settings;
            elasticsearchClient = elasticsearchClientFactory.GetClient();
        }

        public void DeleteAll()
        {
            elasticsearchClient.IndicesDelete(settings.LastTicksIndex).ProcessResponse(200, 404);
            //NOTE без этого разрушает индексы и нужен перезапуск ES
            elasticsearchClient.ClearScroll("_all").ProcessResponse(); //todo плохо, мешает чужим поискам
            elasticsearchClient.IndicesDelete(settings.OldDataIndex).ProcessResponse(200, 404);
            elasticsearchClient.IndicesDelete(settings.IndexPrefix + "*").ProcessResponse(200, 404);

            elasticsearchClient.IndicesDeleteTemplateForAll(settings.TemplateNamePrefix + TaskSearchIndexSchema.DataTemplateSuffix).ProcessResponse(200, 404);
            elasticsearchClient.IndicesDeleteTemplateForAll(settings.TemplateNamePrefix + TaskSearchIndexSchema.OldDataTemplateSuffix).ProcessResponse(200, 404);
            //TODO delete aliases
            elasticsearchClient.ClusterHealth(p => p.WaitForStatus(WaitForStatus.Green)).ProcessResponse();

            Refresh();
        }

        public void Refresh()
        {
            elasticsearchClient.IndicesRefresh("_all");
        }

        private readonly TaskSchemaDynamicSettings settings;
        private readonly IElasticsearchClient elasticsearchClient;
    }
}