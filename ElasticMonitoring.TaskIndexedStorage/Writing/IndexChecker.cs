using System.Collections.Generic;

using Elasticsearch.Net;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing
{
    public class IndexChecker
    {
        public IndexChecker(InternalDataElasticsearchFactory factory)
        {
            elasticsearchClient = factory.DefaultClient.Value;
        }

        public bool CheckAliasExists(string oldIndexName)
        {
            lock(lockObject)
                if(aliasExsitsCache.Contains(oldIndexName))
                    return true;
            if(elasticsearchClient.IndicesExistsAliasForAll(oldIndexName).ProcessResponse(200, 404).HttpStatusCode == 404)
                return false;
            lock(lockObject)
                aliasExsitsCache.Add(oldIndexName);
            return true;
        }

        private readonly object lockObject = new object();
        private readonly HashSet<string> aliasExsitsCache = new HashSet<string>();
        private readonly IElasticsearchClient elasticsearchClient;
    }
}