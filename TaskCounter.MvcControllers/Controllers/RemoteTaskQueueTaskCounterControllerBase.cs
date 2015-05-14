using System.Web.Mvc;

using SKBKontur.Catalogue.Core.Web.Controllers;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.MvcControllers.Models;

using ControllerBase = SKBKontur.Catalogue.Core.Web.Controllers.ControllerBase;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.MvcControllers.Controllers
{
    public abstract class RemoteTaskQueueTaskCounterControllerBase : ControllerBase
    {
        protected RemoteTaskQueueTaskCounterControllerBase(ControllerBaseParameters baseParameters, RemoteTaskQueueTaskCounterControllerImpl controllerImpl)
            : base(baseParameters)
        {
            this.controllerImpl = controllerImpl;
        }

        [HttpGet]
        public ActionResult Run()
        {
            CheckAccess();
            return View("FullScreenTaskCountFast", new TaskCounterModel {GetCountUrl = Url.Action("GetProcessingTaskCount")});
        }

        [HttpGet]
        public JsonResult GetProcessingTaskCount()
        {
            CheckAccess();
            var taskCountModel = controllerImpl.GetProcessingTaskCount();
            return Json(taskCountModel, JsonRequestBehavior.AllowGet);
        }

        protected abstract void CheckAccess();

        private readonly RemoteTaskQueueTaskCounterControllerImpl controllerImpl;
    }
}