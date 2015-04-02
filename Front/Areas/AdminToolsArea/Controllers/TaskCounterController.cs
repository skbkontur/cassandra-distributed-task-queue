using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.MvcControllers.Controllers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.Areas.AdminToolsArea.Controllers
{
    public class TaskCounterController : RemoteTaskQueueTaskCounterControllerBase
    {
        public TaskCounterController(RemoteTaskQueueTaskCounterControllerParameters parameters, RemoteTaskQueueTaskCounterControllerImpl controllerImpl)
            : base(parameters, controllerImpl)
        {
        }

        protected override void CheckAccess()
        {
        }
    }
}