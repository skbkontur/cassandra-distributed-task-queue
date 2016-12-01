using Elasticsearch.Net;

using log4net;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions.Responses.Get;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Utils;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing.Contracts;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing
{
    public class LastReadTicksStorage
    {
        public LastReadTicksStorage(RtqElasticsearchClientFactory elasticsearchClientFactory, ITaskWriteDynamicSettings settings)
        {
            elasticsearchClient = elasticsearchClientFactory.DefaultClient.Value;
            index = settings.LastTicksIndex;
            calculatedIndexStartTimeTicks = settings.CalculatedIndexStartTimeTicks;
            logger.LogInfoFormat("CalculatedIndexStartTimeTicks = {0}", DateTimeFormatter.FormatWithMsAndTicks(calculatedIndexStartTimeTicks));
        }

        public long GetLastReadTicks()
        {
            var response = elasticsearchClient.Get<GetResponse<LastUpdateTicks>>(index, lastUpdateTicksType, id).ProcessResponse();
            if(!response.Response.Found || response.Response.Source == null)
                return calculatedIndexStartTimeTicks;
            return response.Response.Source.Ticks;
        }

        public void SetLastReadTicks(long ticks)
        {
            elasticsearchClient.Index(index, lastUpdateTicksType, id, new LastUpdateTicks {Ticks = ticks}).ProcessResponse();
        }

        private const string lastUpdateTicksType = "LastUpdateTicks";
        private const string id = "LastUpdateTicks";
        private static readonly ILog logger = LogManager.GetLogger("LastReadTicksStorage");
        private readonly IElasticsearchClient elasticsearchClient;
        private readonly string index;
        private readonly long calculatedIndexStartTimeTicks;
    }
}