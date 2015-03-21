using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.MvcControllers.Controllers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.Areas.AdminToolsArea.Controllers
{
    public class TaskCounterController : RemoteTaskQueueTaskCounterControllerBase
    {
        public TaskCounterController(RemoteTaskQueueTaskCounterControllerParameters parameters)
            : base(parameters)
        {
        }

        protected override void CheckAccess()
        {
        }
    }
}