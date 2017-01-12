using System.Collections.Generic;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;

namespace RemoteTaskQueue.Monitoring.Storage.Writing
{
    public class IndexChecker
    {
        public IndexChecker(RtqElasticsearchClientFactory elasticsearchClientFactory)
        {
            this.elasticsearchClientFactory = elasticsearchClientFactory;
        }

        public bool CheckAliasExists([NotNull] string oldIndexName)
        {
            lock(lockObject)
            {
                if(aliasExsitsCache.Contains(oldIndexName))
                    return true;
            }
            if(elasticsearchClientFactory.DefaultClient.Value.IndicesExistsAliasForAll(oldIndexName).ProcessResponse(200, 404).HttpStatusCode == 404)
                return false;
            lock(lockObject)
                aliasExsitsCache.Add(oldIndexName);
            return true;
        }

        private readonly object lockObject = new object();
        private readonly HashSet<string> aliasExsitsCache = new HashSet<string>();
        private readonly RtqElasticsearchClientFactory elasticsearchClientFactory;
    }
}