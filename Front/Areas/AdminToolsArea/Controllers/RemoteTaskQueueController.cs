using SKBKontur.Catalogue.Core.Web.Controllers;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Controllers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.Areas.AdminToolsArea.Controllers
{
    public class RemoteTaskQueueController : RemoteTaskQueueControllerBase
    {
        public RemoteTaskQueueController(ControllerBaseParameters baseParameters, RemoteTaskQueueControllerImpl controllerImpl)
            : base(baseParameters, controllerImpl)
        {
        }

        protected override sealed bool CurrentUserHasAccessToReadAction()
        {
            return true;
        }

        protected override sealed bool CurrentUserHasAccessToTaskData()
        {
            return true;
        }

        protected override sealed bool CurrentUserHasAccessToWriteAction()
        {
            return true;
        }
    }
}