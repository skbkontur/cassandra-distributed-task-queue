using RemoteTaskQueue.Monitoring.Storage.Types;

using SKBKontur.Catalogue.Objects.Json;
using SKBKontur.Catalogue.ServiceLib.HttpHandlers;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class RtqElasticsearchIndexerStatusHttpHandler : IHttpHandler
    {
        public RtqElasticsearchIndexerStatusHttpHandler(ITaskIndexController taskIndexController)
        {
            this.taskIndexController = taskIndexController;
        }

        [HttpMethod]
        public ElasticMonitoringStatus GetStatus()
        {
            return taskIndexController.GetStatus();
        }

        [HttpMethod]
        [JsonHttpMethod]
        public string GetStatusJson()
        {
            return taskIndexController.GetStatus().ToJson();
        }

        private readonly ITaskIndexController taskIndexController;
    }
}