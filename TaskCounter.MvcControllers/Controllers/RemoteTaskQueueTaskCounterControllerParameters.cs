using SKBKontur.Catalogue.Core.Web.Controllers;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Client;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.MvcControllers.Controllers
{
    public class RemoteTaskQueueTaskCounterControllerParameters
    {
        public RemoteTaskQueueTaskCounterControllerParameters(
            ControllerBaseParameters baseParameters,
            IRemoteTaskQueueTaskCounterClient remoteTaskQueueTaskCounterClient
            )
        {
            BaseParameters = baseParameters;
            RemoteTaskQueueTaskCounterClient = remoteTaskQueueTaskCounterClient;
        }

        public ControllerBaseParameters BaseParameters { get; private set; }
        public IRemoteTaskQueueTaskCounterClient RemoteTaskQueueTaskCounterClient { get; private set; }
    }
}