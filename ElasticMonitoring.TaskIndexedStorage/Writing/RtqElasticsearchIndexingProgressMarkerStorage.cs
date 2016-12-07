using System;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions.Responses.Get;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing.Contracts;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing
{
    public class RtqElasticsearchIndexingProgressMarkerStorage : IRtqElasticsearchIndexingProgressMarkerStorage
    {
        public RtqElasticsearchIndexingProgressMarkerStorage(RtqElasticsearchClientFactory elasticsearchClientFactory)
        {
            this.elasticsearchClientFactory = elasticsearchClientFactory;
            IndexingFinishTimestamp = null;
        }

        [CanBeNull]
        public Timestamp IndexingFinishTimestamp { get; private set; }

        public long GetLastReadTicks()
        {
            var response = elasticsearchClientFactory.DefaultClient.Value.Get<GetResponse<LastUpdateTicks>>(RtqElasticsearchConsts.IndexingProgressIndex, typeof(LastUpdateTicks).Name, id).ProcessResponse();
            if(!response.Response.Found || response.Response.Source == null)
                return indexingStartTimestamp.Ticks;
            return response.Response.Source.Ticks;
        }

        public void SetLastReadTicks(long ticks)
        {
            elasticsearchClientFactory.DefaultClient.Value.Index(RtqElasticsearchConsts.IndexingProgressIndex, typeof(LastUpdateTicks).Name, id, new LastUpdateTicks {Ticks = ticks}).ProcessResponse();
        }

        private const string id = "LastUpdateTicksKey";
        private static readonly Timestamp indexingStartTimestamp = new Timestamp(new DateTime(2016, 01, 01, 0, 0, 0, DateTimeKind.Utc));
        private readonly RtqElasticsearchClientFactory elasticsearchClientFactory;
    }
}