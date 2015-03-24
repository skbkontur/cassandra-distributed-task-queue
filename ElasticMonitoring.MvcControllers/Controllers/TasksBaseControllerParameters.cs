using RemoteQueue.Handling;
using RemoteQueue.UserClasses;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.Core.Web.Controllers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers
{
    public class TasksBaseControllerParameters
    {
        public TasksBaseControllerParameters(
            ControllerBaseParameters baseParameters,
            IElasticsearchClientFactory elasticsearchClientFactory,
            TaskDataRegistryBase taskDataRegistryBase,
            IRemoteTaskQueue remoteTaskQueue)
        {
            BaseParameters = baseParameters;
            ElasticsearchClientFactory = elasticsearchClientFactory;
            TaskDataRegistryBase = taskDataRegistryBase;
            RemoteTaskQueue = remoteTaskQueue;
        }

        public ControllerBaseParameters BaseParameters { get; private set; }
        public IElasticsearchClientFactory ElasticsearchClientFactory { get; private set; }
        public TaskDataRegistryBase TaskDataRegistryBase { get; private set; }
        public IRemoteTaskQueue RemoteTaskQueue { get; set; }
    }
}