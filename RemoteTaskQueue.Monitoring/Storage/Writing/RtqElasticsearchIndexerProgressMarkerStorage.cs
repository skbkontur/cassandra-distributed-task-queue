using System;

using JetBrains.Annotations;

using RemoteTaskQueue.Monitoring.Storage.Writing.Contracts;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions.Responses.Get;
using SKBKontur.Catalogue.Objects;

namespace RemoteTaskQueue.Monitoring.Storage.Writing
{
    public class RtqElasticsearchIndexerProgressMarkerStorage : IRtqElasticsearchIndexerProgressMarkerStorage
    {
        public RtqElasticsearchIndexerProgressMarkerStorage(RtqElasticsearchClientFactory elasticsearchClientFactory)
        {
            this.elasticsearchClientFactory = elasticsearchClientFactory;
            IndexingFinishTimestamp = null;
        }

        [CanBeNull]
        public Timestamp IndexingFinishTimestamp { get; private set; }

        [NotNull]
        public Timestamp GetIndexingStartTimestamp()
        {
            var response = elasticsearchClientFactory.DefaultClient.Value.Get<GetResponse<LastUpdateTicks>>(RtqElasticsearchConsts.IndexingProgressIndex, typeof(LastUpdateTicks).Name, id).ProcessResponse();
            if(!response.Response.Found || response.Response.Source == null)
                return indexingStartTimestamp;
            return new Timestamp(response.Response.Source.Ticks);
        }

        public void SetIndexingStartTimestamp([NotNull] Timestamp newIndexigStartTimestamp)
        {
            elasticsearchClientFactory.DefaultClient.Value.Index(RtqElasticsearchConsts.IndexingProgressIndex, typeof(LastUpdateTicks).Name, id, new LastUpdateTicks {Ticks = newIndexigStartTimestamp.Ticks}).ProcessResponse();
        }

        private const string id = "LastUpdateTicksKey";
        private static readonly Timestamp indexingStartTimestamp = new Timestamp(new DateTime(2016, 01, 01, 0, 0, 0, DateTimeKind.Utc));
        private readonly RtqElasticsearchClientFactory elasticsearchClientFactory;
    }
}