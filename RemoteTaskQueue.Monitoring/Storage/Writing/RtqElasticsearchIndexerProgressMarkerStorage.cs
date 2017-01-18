using JetBrains.Annotations;

using RemoteTaskQueue.Monitoring.Indexer;
using RemoteTaskQueue.Monitoring.Storage.Writing.Contracts;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions.Responses.Get;
using SKBKontur.Catalogue.Objects;

namespace RemoteTaskQueue.Monitoring.Storage.Writing
{
    public class RtqElasticsearchIndexerProgressMarkerStorage : IRtqElasticsearchIndexerProgressMarkerStorage
    {
        public RtqElasticsearchIndexerProgressMarkerStorage(RtqElasticsearchClientFactory elasticsearchClientFactory, RtqElasticsearchIndexerSettings settings)
        {
            this.elasticsearchClientFactory = elasticsearchClientFactory;
            this.settings = settings;
            IndexingFinishTimestamp = null;
        }

        [CanBeNull]
        public Timestamp IndexingFinishTimestamp { get; private set; }

        [NotNull]
        public Timestamp GetIndexingStartTimestamp()
        {
            var response = elasticsearchClientFactory.DefaultClient.Value.Get<GetResponse<LastUpdateTicks>>(RtqElasticsearchConsts.IndexingProgressIndex, typeof(LastUpdateTicks).Name, id).ProcessResponse();
            if(!response.Response.Found || response.Response.Source == null)
                return settings.InitialIndexingStartTimestamp;
            return new Timestamp(response.Response.Source.Ticks);
        }

        public void SetIndexingStartTimestamp([NotNull] Timestamp newIndexigStartTimestamp)
        {
            elasticsearchClientFactory.DefaultClient.Value.Index(RtqElasticsearchConsts.IndexingProgressIndex, typeof(LastUpdateTicks).Name, id, new LastUpdateTicks {Ticks = newIndexigStartTimestamp.Ticks}).ProcessResponse();
        }

        private const string id = "LastUpdateTicksKey";
        private readonly RtqElasticsearchClientFactory elasticsearchClientFactory;
        private readonly RtqElasticsearchIndexerSettings settings;
    }
}