using Elasticsearch.Net;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Search;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Utils;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing;
using SKBKontur.Catalogue.ServiceLib.Logging;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation
{
    public class TaskIndexCloseService
    {
        public TaskIndexCloseService(RtqElasticsearchClientFactory elasticsearchClientFactory)
        {
            this.elasticsearchClientFactory = elasticsearchClientFactory;
        }

        private bool AdjustIndexState([NotNull] string index)
        {
            var elasticsearchClient = elasticsearchClientFactory.DefaultClient.Value;
            var response = elasticsearchClient.CatIndices(index, p => p.H("status")).ProcessResponse(200, 404);
            if(response.HttpStatusCode == 404)
            {
                Log.For(this).LogInfoFormat("Index = '{0}' not exists - creating it", index);
                var respCreate = elasticsearchClient.IndicesCreate(index, new {}).ProcessResponse(200, 400);
                if(!(respCreate.HttpStatusCode == 400 && respCreate.ServerError != null && respCreate.ServerError.ExceptionType == "IndexAlreadyExistsException"))
                    respCreate.ProcessResponse(); //note throw any other error
                return true;
            }
            var state = response.Response;
            return state.Trim('\r', '\n', ' ') == "open";
        }

        public void CloseOldIndices([NotNull] Timestamp from, [NotNull] Timestamp to)
        {
            var maxTo = Timestamp.Now - WriteIndexNameFactory.OldTaskInterval;
            if(to > maxTo)
                to = maxTo;
            Log.For(this).LogInfoFormat("Closing old indices. timeFrom={0} timeTo={1}", from, to);
            var indexForTimeRange = SearchIndexNameFactory.GetIndexForTimeRange(from.Ticks, to.Ticks, WriteIndexNameFactory.CurrentIndexNameFormat);
            var indexNames = indexForTimeRange.Split(',');
            Log.For(this).LogInfoFormat("Indices to process: {0}", indexNames.Length);
            foreach(var indexName in indexNames)
                CloseIndex(indexName);
            Log.For(this).LogInfoFormat("Closing Old indices done");
        }

        private void CloseIndex([NotNull] string indexName)
        {
            if(!AdjustIndexState(indexName))
                Log.For(this).LogInfoFormat("Index = '{0}' already closed", indexName);
            Log.For(this).LogInfoFormat("Redirecting aliases for index = '{0}'", indexName);
            var oldDataAlias = IndexNameConverter.FillIndexNamePlaceholder(RtqElasticsearchConsts.OldDataAliasFormat, indexName);
            var searchAlias = IndexNameConverter.FillIndexNamePlaceholder(RtqElasticsearchConsts.SearchAliasFormat, indexName);
            var body = new
                {
                    actions = new object[]
                        {
                            new {remove = new {index = indexName, alias = oldDataAlias}},
                            new {add = new {index = RtqElasticsearchConsts.OldDataIndex, alias = oldDataAlias}},
                            new {remove = new {index = indexName, alias = searchAlias}},
                            new {add = new {index = RtqElasticsearchConsts.OldDataIndex, alias = searchAlias}},
                        }
                };
            var elasticsearchClient = elasticsearchClientFactory.DefaultClient.Value;
            elasticsearchClient.IndicesUpdateAliasesForAll(body).ProcessResponse();
            Log.For(this).LogInfoFormat("Waiting for green status index = '{0}'", indexName);
            elasticsearchClient.ClusterHealth(indexName, p => p.WaitForStatus(WaitForStatus.Green)).ProcessResponse();
            Log.For(this).LogInfoFormat("Closing index = '{0}'", indexName);
            elasticsearchClient.IndicesClose(indexName).ProcessResponse();
        }

        private readonly RtqElasticsearchClientFactory elasticsearchClientFactory;
    }
}