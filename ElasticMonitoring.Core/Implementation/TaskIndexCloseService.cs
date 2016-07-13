using System;

using Elasticsearch.Net;

using log4net;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.Utils;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Search;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Utils;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation
{
    public class TaskIndexCloseService
    {
        public TaskIndexCloseService(SearchIndexNameFactory searchIndexNameFactory, IElasticsearchClientFactory elasticsearchClientFactory, ITaskWriteDynamicSettings taskWriteDynamicSettings)
        {
            this.searchIndexNameFactory = searchIndexNameFactory;
            this.taskWriteDynamicSettings = taskWriteDynamicSettings;
            elasticsearchClient = elasticsearchClientFactory.GetClient();
        }

        private bool AdjustIndexState(string index)
        {
            var response = elasticsearchClient.ClusterState("metadata", index).ProcessResponse(200, 404);
            foreach(var obj in response.Response["metadata"]["indices"])
            {
                if(obj.Name == index)
                {
                    if(obj.Value.state.ToString() == "open")
                        return true;
                    if(obj.Value.state.ToString() == "close")
                        return false;
                }
            }
            logger.InfoFormat("Index = '{0}' not exists - creating it", index);
            elasticsearchClient.IndicesCreate(index, new {}).ProcessResponse();
            return true;
        }

        public void CloseOldIndices(DateTime from, DateTime to)
        {
            var maxTo = DateTime.UtcNow - WriteIndexNameFactory.OldTaskInterval;
            if(to > maxTo)
                to = maxTo;
            logger.InfoFormat("Closing old indices. timeFrom={0} timeTo={1}", DateTimeFormatter.FormatWithMs(from), DateTimeFormatter.FormatWithMs(to));
            var indexForTimeRange = searchIndexNameFactory.GetIndexForTimeRange(@from.Ticks, to.Ticks, taskWriteDynamicSettings.CurrentIndexNameFormat);
            var indexNames = indexForTimeRange.Split(',');
            logger.InfoFormat("Indices to process: {0}", indexNames.Length);
            foreach(var indexName in indexNames)
            {
                CloseIndex(indexName);
            }
            logger.InfoFormat("Closing Old indices done");
        }

        public void CloseIndex(string indexName)
        {
            if(!AdjustIndexState(indexName))
                logger.InfoFormat("Index = '{0}' already closed", indexName);
            else
            {
                logger.InfoFormat("Redirecting aliases for index = '{0}'", indexName);
                var oldDataAlias = IndexNameConverter.FillIndexNamePlaceholder(taskWriteDynamicSettings.OldDataAliasFormat, indexName);
                var searchAlias = IndexNameConverter.FillIndexNamePlaceholder(taskWriteDynamicSettings.SearchAliasFormat, indexName);
                var body = new
                    {
                        actions = new object[]
                            {
                                new {remove = new {index = indexName, alias = oldDataAlias}},
                                new {add = new {index = taskWriteDynamicSettings.OldDataIndex, alias = oldDataAlias}},
                                new {remove = new {index = indexName, alias = searchAlias}},
                                new {add = new {index = taskWriteDynamicSettings.OldDataIndex, alias = searchAlias}},
                            }
                    };
                elasticsearchClient.IndicesUpdateAliasesForAll(body).ProcessResponse();
                logger.InfoFormat("Closing index = '{0}'", indexName);
                elasticsearchClient.IndicesClose(indexName);
            }
        }

        private static readonly ILog logger = LogManager.GetLogger("TaskIndexCloseService");

        private readonly SearchIndexNameFactory searchIndexNameFactory;
        private readonly ITaskWriteDynamicSettings taskWriteDynamicSettings;
        private readonly IElasticsearchClient elasticsearchClient;
    }
}