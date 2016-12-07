using SKBKontur.Catalogue.Objects.Json;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Types;
using SKBKontur.Catalogue.ServiceLib.HttpHandlers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core
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