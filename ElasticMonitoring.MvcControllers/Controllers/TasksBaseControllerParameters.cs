using RemoteQueue.Handling;
using RemoteQueue.UserClasses;

using SKBKontur.Catalogue.Core.Web.Controllers;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Client;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers
{
    public class TasksBaseControllerParameters
    {
        public TasksBaseControllerParameters(
            ControllerBaseParameters baseParameters,
            ITaskSearchClient taskSearchClient,
            TaskDataRegistryBase taskDataRegistryBase,
            IRemoteTaskQueue remoteTaskQueue)
        {
            BaseParameters = baseParameters;
            TaskSearchClient = taskSearchClient;
            TaskDataRegistryBase = taskDataRegistryBase;
            RemoteTaskQueue = remoteTaskQueue;
        }

        public ControllerBaseParameters BaseParameters { get; private set; }
        public ITaskSearchClient TaskSearchClient { get; private set; }
        public TaskDataRegistryBase TaskDataRegistryBase { get; private set; }
        public IRemoteTaskQueue RemoteTaskQueue { get; private set; }
    }
}