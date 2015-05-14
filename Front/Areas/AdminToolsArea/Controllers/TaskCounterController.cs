using SKBKontur.Catalogue.Core.Web.Controllers;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.MvcControllers.Controllers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.Areas.AdminToolsArea.Controllers
{
    public class TaskCounterController : RemoteTaskQueueTaskCounterControllerBase
    {
        public TaskCounterController(ControllerBaseParameters baseParameters, RemoteTaskQueueTaskCounterControllerImpl controllerImpl)
            : base(baseParameters, controllerImpl)
        {
        }

        protected override void CheckAccess()
        {
        }
    }
}