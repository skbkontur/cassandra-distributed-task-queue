using System;

using Elasticsearch.Net;

using log4net;

using Newtonsoft.Json;

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

        public void CloseOldIndices(DateTime from, DateTime to)
        {
            elasticsearchClient.IndicesRefresh("_all");
            var maxTo = DateTime.UtcNow - WriteIndexNameFactory.OldTaskInterval;
            if(to > maxTo)
                to = maxTo;
            logger.InfoFormat("Closing indices. timeFrom={0} timeTo={1}", DateTimeFormatter.FormatWithMs(from), DateTimeFormatter.FormatWithMs(to));
            var indexForTimeRange = searchIndexNameFactory.GetIndexForTimeRange(@from.Ticks, to.Ticks, taskWriteDynamicSettings.CurrentIndexNameFormat);
            var indexNames = indexForTimeRange.Split(',');
            logger.InfoFormat("Indices found: {0}", indexNames.Length);
            foreach(var indexName in indexNames)
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
            }
            logger.InfoFormat("Closing indices");
            elasticsearchClient.IndicesClose(indexForTimeRange).ProcessResponse();
            logger.InfoFormat("Closing indices done");
        }

        private static readonly ILog logger = LogManager.GetLogger("TaskIndexCloseService");

        private readonly SearchIndexNameFactory searchIndexNameFactory;
        private readonly ITaskWriteDynamicSettings taskWriteDynamicSettings;
        private readonly IElasticsearchClient elasticsearchClient;
    }
}