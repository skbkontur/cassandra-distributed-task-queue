using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

using Humanizer;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Configuration;

using SKBKontur.Catalogue.Core.Web.Controllers;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Models;

using ControllerBase = SKBKontur.Catalogue.Core.Web.Controllers.ControllerBase;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers
{
    public abstract class TasksBaseController : ControllerBase
    {
        protected TasksBaseController(ControllerBaseParameters baseParameters, TasksControllerImpl controllerImpl, ITaskDataRegistry taskDataRegistry)
            : base(baseParameters)
        {
            this.controllerImpl = controllerImpl;
            taskDataNames = taskDataRegistry.GetAllTaskDataInfos().OrderBy(x => x.Value).Select(x => x.Value).Distinct().ToArray();
            taskStates = Enum.GetValues(typeof(TaskState)).Cast<TaskState>().Select(x => new KeyValuePair<string, string>(x.ToString(), x.Humanize())).ToArray();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Run(string q, string[] name, string[] state, string start, string end)
        {
            CheckReadAccess();

            var rangeStart = controllerImpl.ParseDateTime(start);
            var rangeEnd = controllerImpl.ParseDateTime(end);
            var taskSearchConditions = new TaskSearchConditionsModel
                {
                    SearchString = q,
                    TaskNames = name,
                    TaskStates = state,
                    RangeStart = rangeStart,
                    RangeEnd = rangeEnd,
                    AdminToolAction = GetAdminToolsActions(),
                    AvailableTaskDataNames = taskDataNames,
                    AvailableTaskStates = taskStates
                };
            if(!taskSearchConditions.RangeStart.HasValue || !taskSearchConditions.RangeEnd.HasValue)
                return View("SearchPage", taskSearchConditions);

            if(string.IsNullOrWhiteSpace(q))
                return RedirectToAction("Run", new RouteValueDictionary(new {q = "*", start, end}).AppendArray(name, "name").AppendArray(state, "state"));

            var taskSearchResultsModel = controllerImpl.BuildResultsBySearchConditions(taskSearchConditions, CurrentUserHasAccessToWriteAction(), CurrentUserHasAccessToTaskData());
            return View("SearchResultsPage", new TasksResultModel
                {
                    SearchConditions = taskSearchConditions,
                    Results = taskSearchResultsModel
                });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ViewResult Scroll(string iteratorContext)
        {
            CheckReadAccess();
            var taskSearchResultsModel = controllerImpl.BuildResultsByIteratorContext(iteratorContext, CurrentUserHasAccessToWriteAction(), CurrentUserHasAccessToTaskData());
            return View("Scroll", taskSearchResultsModel);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ViewResult Details(string id)
        {
            CheckReadAccess();
            var detailsModel = controllerImpl.Details(id, Url, CurrentUserHasAccessToWriteAction(), CurrentUserHasAccessToTaskData());
            return View("TaskDetails2", detailsModel);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetBytes(string id, string path)
        {
            CheckReadAccess();
            string fileDownloadName;
            var fileContents = controllerImpl.GetBytes(id, path, out fileDownloadName);
            return File(fileContents, "application/octet-stream", fileDownloadName);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Cancel(string id)
        {
            CheckWriteAccess();
            controllerImpl.Cancel(id);
            return Json(new {Success = true});
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Rerun(string id)
        {
            CheckWriteAccess();
            controllerImpl.Rerun(id);
            return Json(new {Success = true});
        }

        private void CheckWriteAccess()
        {
            if(!CurrentUserHasAccessToWriteAction())
                throw new ForbiddenException(Request.RawUrl, null);
        }

        private void CheckReadAccess()
        {
            if(!CurrentUserHasAccessToReadAction())
                throw new ForbiddenException(Request.RawUrl, null);
        }

        protected abstract string GetAdminToolsActions();
        protected abstract bool CurrentUserHasAccessToReadAction();
        protected abstract bool CurrentUserHasAccessToTaskData();
        protected abstract bool CurrentUserHasAccessToWriteAction();

        private readonly TasksControllerImpl controllerImpl;
        private readonly string[] taskDataNames;
        private readonly KeyValuePair<string, string>[] taskStates;
    }
}