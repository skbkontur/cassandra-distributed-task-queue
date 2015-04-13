using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

using Humanizer;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.Core.Web.Controllers;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.Objects.ValueExtracting;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.TaskDetailsTreeView;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Models;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Client;

using ControllerBase = SKBKontur.Catalogue.Core.Web.Controllers.ControllerBase;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers
{
    public abstract class TasksBaseController : ControllerBase
    {
        protected TasksBaseController(
            TasksBaseControllerParameters parameters)
            : base(parameters.BaseParameters)
        {
            taskSearchClient = parameters.TaskSearchClient;
            taskDataNames = parameters.TaskDataRegistryBase.GetAllTaskDataInfos().OrderBy(x => x.Value).Select(x => x.Value).Distinct().ToArray();
            taskStates = Enum.GetValues(typeof(TaskState)).Cast<TaskState>().Select(x => new KeyValuePair<string, string>(x.ToString(), x.Humanize())).ToArray();
            remoteTaskQueue = parameters.RemoteTaskQueue;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Run(string q, string[] name, string[] state, string start, string end)
        {
            CheckReadAccess();

            var rangeStart = ParseDateTime(start);
            var rangeEnd = ParseDateTime(end);
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

            var taskSearchResultsModel = BuildResultsBySearchConditions(taskSearchConditions);
            return View("SearchResultsPage", new TasksResultModel
                {
                    SearchConditions = taskSearchConditions,
                    Results = taskSearchResultsModel
                });
        }

        public static byte[] Compress(byte[] raw)
        {
            using(var memory = new MemoryStream())
            {
                using(var gzip = new GZipStream(memory, CompressionMode.Compress, true))
                    gzip.Write(raw, 0, raw.Length);

                return memory.ToArray();
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ViewResult Scroll(string iteratorContext)
        {
            CheckReadAccess();
            var taskSearchResultsModel = BuildResultsByIteratorContext(iteratorContext);
            return View("Scroll", taskSearchResultsModel);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ViewResult Details(string id)
        {
            CheckReadAccess();
            var taskData = remoteTaskQueue.GetTaskInfo(id);
            return View("TaskDetails2", new TaskDetailsModel
                {
                    AllowControlTaskExecution = CurrentUserHasAccessToWriteAction(),
                    TaskName = taskData.Context.Name,
                    TaskId = taskData.Context.Id,
                    State = taskData.Context.State,
                    EnqueueTime = new DateTime(taskData.Context.Ticks, DateTimeKind.Utc),
                    StartExecutedTime = TickToDateTime(taskData.Context.StartExecutingTicks),
                    FinishExecutedTime = TickToDateTime(taskData.Context.FinishExecutingTicks),
                    MinimalStartTime = TickToDateTime(taskData.Context.MinimalStartTicks),
                    ParentTaskId = taskData.Context.ParentTaskId,
                    ChildTaskIds = remoteTaskQueue.GetChildrenTaskIds(taskData.Context.Id),
                    ExceptionInfo = taskData.With(x => x.ExceptionInfo).Return(x => x.ExceptionMessageInfo, ""),
                    AttemptCount = taskData.Context.Attempts,
                    DetailsTree = BuildDetailsTree(taskData, id)
                });
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetBytes(string id, string path)
        {
            CheckReadAccess();
            var taskData = remoteTaskQueue.GetTaskInfo(id).TaskData;
            var value = ObjectValueExtractor.Extract(taskData.GetType(), taskData, path);
            if(value.GetType() != typeof(byte[]))
                throw new Exception(string.Format("Type of property by path '{0}' has type '{1}' instead of '{2}'", path, value.GetType(), typeof(byte[])));
            var fileDownloadName = string.Format("{0}_{1}_{2}.data", DateTime.UtcNow.ToString("yyyy.MM.dd hh:mm:ss"), path, id);
            return File((byte[])value, "application/octet-stream", fileDownloadName);
        }

        private static DateTime? TickToDateTime(long? startExecutingTicks)
        {
            if(!startExecutingTicks.HasValue)
                return null;
            return new DateTime(startExecutingTicks.Value, DateTimeKind.Utc);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Cancel(string id)
        {
            CheckWriteAccess();
            remoteTaskQueue.CancelTask(id);
            return Json(new {Success = true});
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Rerun(string id)
        {
            CheckWriteAccess();
            remoteTaskQueue.RerunTask(id, TimeSpan.FromTicks(0));
            return Json(new {Success = true});
        }

        private void CheckWriteAccess()
        {
            if (!CurrentUserHasAccessToWriteAction())
                throw new ForbiddenException(Request.RawUrl, null);
        }

        private void CheckReadAccess()
        {
            if (!CurrentUserHasAccessToReadAction())
                throw new ForbiddenException(Request.RawUrl, null);
        }

        protected abstract string GetAdminToolsActions();

        protected abstract bool CurrentUserHasAccessToReadAction();

        protected abstract bool CurrentUserHasAccessToTaskData();

        protected abstract bool CurrentUserHasAccessToWriteAction();

        private ObjectTreeModel BuildDetailsTree(RemoteTaskInfo taskData, string taskId)
        {
            if(!CurrentUserHasAccessToTaskData())
                return null;
            var builder = new ObjectTreeViewBuilder(new TaskDataBuildersProvider());
            var result = builder.Build(taskData.TaskData, new TaskDataBuildingContext {TaskId = taskId, UrlHelper = Url});
            if(result != null)
                result.Name = "TaskData";
            return result;
        }

        private TaskSearchResultsModel BuildResultsByIteratorContext(string iteratorContext)
        {
            var taskSearchResponse = taskSearchClient.SearchNext(iteratorContext);

            var tasksIds = taskSearchResponse.Ids;
            var nextScrollId = taskSearchResponse.NextScrollId;
            return new TaskSearchResultsModel
                {
                    AllowControlTaskExecution = CurrentUserHasAccessToWriteAction(),
                    AllowViewTaskData = CurrentUserHasAccessToTaskData(),
                    Tasks = remoteTaskQueue.GetTaskInfos(tasksIds).Select(x => new TaskModel
                        {
                            Id = x.Context.Id,
                            Name = x.Context.Name,
                            State = x.Context.State,
                            EnqueueTime = FromTicks(x.Context.Ticks),
                            StartExecutionTime = FromTicks(x.Context.StartExecutingTicks),
                            FinishExecutionTime = FromTicks(x.Context.FinishExecutingTicks),
                            MinimalStartTime = FromTicks(x.Context.MinimalStartTicks),
                            AttemptCount = x.Context.Attempts,
                            ParentTaskId = x.Context.ParentTaskId,
                        }).ToArray(),
                    IteratorContext = nextScrollId,
                };
        }

        private TaskSearchResultsModel BuildResultsBySearchConditions(TaskSearchConditionsModel taskSearchConditions)
        {
            if(taskSearchConditions.RangeStart == null)
                throw new Exception("Range start should be specified");
            var start = taskSearchConditions.RangeStart.Value.Ticks;
            var end = (taskSearchConditions.RangeEnd ?? DateTime.UtcNow).Ticks;
            var taskSearchResponse = taskSearchClient.SearchFirst(new TaskSearchRequest
                {
                    FromTicksUtc = start,
                    ToTicksUtc = end,
                    QueryString = taskSearchConditions.SearchString,
                    TaskNames = taskSearchConditions.TaskNames,
                    TaskStates = taskSearchConditions.TaskStates
                });

            var tasksIds = taskSearchResponse.Ids;
            var total = taskSearchResponse.TotalCount;
            var nextScrollId = taskSearchResponse.NextScrollId;

            var taskSearchResultsModel = new TaskSearchResultsModel
                {
                    AllowControlTaskExecution = CurrentUserHasAccessToWriteAction(),
                    AllowViewTaskData = CurrentUserHasAccessToTaskData(),
                    Tasks = remoteTaskQueue.GetTaskInfos(tasksIds).Select(x => new TaskModel
                        {
                            Id = x.Context.Id,
                            Name = x.Context.Name,
                            State = x.Context.State,
                            EnqueueTime = FromTicks(x.Context.Ticks),
                            StartExecutionTime = FromTicks(x.Context.StartExecutingTicks),
                            FinishExecutionTime = FromTicks(x.Context.FinishExecutingTicks),
                            MinimalStartTime = FromTicks(x.Context.MinimalStartTicks),
                            AttemptCount = x.Context.Attempts,
                            ParentTaskId = x.Context.ParentTaskId,
                        }).ToArray(),
                    IteratorContext = nextScrollId,
                    TotalResultCount = total
                };
            return taskSearchResultsModel;
        }

        private static DateTime? ParseDateTime(string start)
        {
            if(string.IsNullOrWhiteSpace(start))
                return null;
            DateTime result;
            if(!DateTime.TryParse(start, CultureInfo.GetCultureInfo("ru-RU"), DateTimeStyles.AssumeUniversal, out result))
                return null;
            return result;
        }

        private static DateTime? FromTicks(long? ticks)
        {
            if(ticks.HasValue)
                return new DateTime(ticks.Value, DateTimeKind.Utc);
            return null;
        }

        private readonly ITaskSearchClient taskSearchClient;
        private readonly string[] taskDataNames;
        private readonly IRemoteTaskQueue remoteTaskQueue;
        private readonly KeyValuePair<string, string>[] taskStates;
    }
}