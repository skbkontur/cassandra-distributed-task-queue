using System.Net;
using System.Web.Mvc;

using SKBKontur.Catalogue.Core.Web.Controllers;
using SKBKontur.Catalogue.Core.Web.PageModels;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskList;

using ControllerBase = SKBKontur.Catalogue.Core.Web.Controllers.ControllerBase;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Controllers
{
    public abstract class RemoteTaskQueueControllerBase : ControllerBase
    {
        protected RemoteTaskQueueControllerBase(ControllerBaseParameters baseParameters, RemoteTaskQueueControllerImpl controllerImpl)
            : base(baseParameters)
        {
            this.controllerImpl = controllerImpl;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [RequireReadAccessToRemoteTaskQueue]
        public ActionResult Run(string searchRequestId, int pageNumber = 0)
        {
            var pageModel = controllerImpl.GetTaskListPageModel(PageModelBaseParameters, new PageModelContext(LanguageProvider), searchRequestId, pageNumber);
            return View("TaskList", pageModel);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [RequireReadAccessToRemoteTaskQueue]
        public ActionResult GetCount(string searchRequestId)
        {
            var totalCount = controllerImpl.GetTotalCount(searchRequestId);
            return Json(totalCount, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [RequireReadAccessToRemoteTaskQueue]
        public ActionResult Search(TaskListModelData pageModelData)
        {
            var searchRequestId = controllerImpl.Search(pageModelData);
            return Json(new SuccessOperationResult
                {
                    NeedRedirect = true,
                    RedirectTo = Url.Action("Run", new {searchRequestId}),
                });
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [RequireReadAccessToRemoteTaskQueue]
        public ActionResult Show(string id, int pageNumber = 0, string searchRequestId = null)
        {
            var pageModel = controllerImpl.GetTaskDetailsPageModel(PageModelBaseParameters, id, pageNumber, searchRequestId, CurrentUserHasAccessToTaskData());
            pageModel.TaskListUrl = Url.Action("Run", new {pageNumber, searchRequestId});
            return View("TaskDetails", pageModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [RequireWriteAccessToRemoteTaskQueue]
        public ActionResult Cancel(string id)
        {
            var result = controllerImpl.Cancel(id);
            if(result.NeedRedirect)
                result.RedirectTo = Request.Headers["Referer"];
            return ReturnOperationResult(result);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [RequireWriteAccessToRemoteTaskQueue]
        public ActionResult Rerun(string id)
        {
            var result = controllerImpl.Rerun(id);
            if(result.NeedRedirect)
                result.RedirectTo = Request.Headers["Referer"];
            return ReturnOperationResult(result);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [RequireReadAccessToRemoteTaskQueue]
        public ActionResult GetBytes(string id, string path)
        {
            string fileDownloadName;
            var bytes = controllerImpl.GetBytes(id, path, out fileDownloadName);
            return File(bytes, "application/octet-stream", fileDownloadName);
        }

        protected abstract bool CurrentUserHasAccessToReadAction();

        protected abstract bool CurrentUserHasAccessToTaskData();

        protected abstract bool CurrentUserHasAccessToWriteAction();

        private readonly RemoteTaskQueueControllerImpl controllerImpl;

        private class RequireReadAccessToRemoteTaskQueueAttribute : ActionFilterAttribute
        {
            public RequireReadAccessToRemoteTaskQueueAttribute()
            {
                Order = 100;
            }

            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                base.OnActionExecuting(filterContext);
                var remoteTaskQueueController = (RemoteTaskQueueControllerBase)filterContext.Controller;
                if(!remoteTaskQueueController.CurrentUserHasAccessToReadAction())
                    filterContext.Result = new HttpStatusCodeResult((int)HttpStatusCode.Forbidden, "Текущий пользователь не имеет прав на чтение состояния очереди");
            }
        }

        private class RequireWriteAccessToRemoteTaskQueueAttribute : ActionFilterAttribute
        {
            public RequireWriteAccessToRemoteTaskQueueAttribute()
            {
                Order = 100;
            }

            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                base.OnActionExecuting(filterContext);
                var remoteTaskQueueController = (RemoteTaskQueueControllerBase)filterContext.Controller;
                if(!remoteTaskQueueController.CurrentUserHasAccessToWriteAction())
                    filterContext.Result = new HttpStatusCodeResult((int)HttpStatusCode.Forbidden, "Текущий пользователь не имеет прав на изменение состояния очереди");
            }
        }
    }
}