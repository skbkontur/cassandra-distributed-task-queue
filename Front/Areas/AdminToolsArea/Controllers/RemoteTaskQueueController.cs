using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Controllers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.Areas.AdminToolsArea.Controllers
{
    public class RemoteTaskQueueController : RemoteTaskQueueControllerBase
    {
        public RemoteTaskQueueController(RemoteTaskQueueControllerBaseParameters remoteTaskQueueControllerBaseParameters)
            : base(remoteTaskQueueControllerBaseParameters)
        {
        }

        protected override sealed bool CurrentUserHasAccessToReadAction()
        {
            return true;
        }

        protected override sealed bool CurrentUserHasAccessToWriteAction()
        {
            return true;
        }
    }
}