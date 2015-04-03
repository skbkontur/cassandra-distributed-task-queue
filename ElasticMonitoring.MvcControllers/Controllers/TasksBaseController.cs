using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

using Humanizer;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Models;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Client;

using ControllerBase = SKBKontur.Catalogue.Core.Web.Controllers.ControllerBase;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers
{
    public abstract class TasksBaseController : ControllerBase
    {
        public TasksBaseController(
            TasksBaseControllerParameters parameters)
            : base(parameters.BaseParameters)
        {
            taskSearchClient = parameters.TaskSearchClient;
            taskDataNames = parameters.TaskDataRegistryBase.GetAllTaskDataInfos().Select(x => x.Value).Distinct().ToArray();
            taskStates = Enum.GetValues(typeof(TaskState)).Cast<TaskState>().Select(x => new KeyValuePair<string, string>(x.ToString(), x.Humanize())).ToArray();
            remoteTaskQueue = parameters.RemoteTaskQueue;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ViewResult Run(string q, string[] name, string[] state, string start, string end)
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
                    AvailableTaskDataNames = taskDataNames,
                    AvailableTaskStates = taskStates
                };

            if(!SearchConditionSuitableForSearch(taskSearchConditions))
                return View("SearchPage", taskSearchConditions);

            var taskSearchResultsModel = BuildResultsBySearchConditions(taskSearchConditions);
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
            var taskSearchResultsModel = BuildResultsByIteratorContext(iteratorContext);
            return View("Scroll", taskSearchResultsModel);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ViewResult Details(string id)
        {
            CheckTaskDataAccess();
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
                    DetailsTree = BuildDetailsTree(taskData)
                });
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

        private void CheckTaskDataAccess()
        {
            if (!CurrentUserHasAccessToTaskData())
                throw new ForbiddenException(Request.RawUrl, null);
        }

        protected abstract bool CurrentUserHasAccessToReadAction();

        protected abstract bool CurrentUserHasAccessToTaskData();

        protected abstract bool CurrentUserHasAccessToWriteAction();

        private ObjectTreeModel BuildDetailsTree(RemoteTaskInfo taskData)
        {
            var result = new ObjectTreeModel();
            var taskDataModel = new ObjectTreeModel
                {
                    Name = "TaskData"
                };
            AddPropertyRecursively(taskDataModel, taskData.TaskData);
            result.AddChild(taskDataModel);
            return result;
        }

        private void AddPropertyRecursively(ObjectTreeModel result, object context)
        {
            var plainTypes = new[]
                {
                    typeof(int),
                    typeof(string)
                };
            if(context == null)
            {
                result.Value = "NULL";
                return;
            }
            foreach(var property in context.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var type = property.PropertyType;
                if(type.IsEnum || plainTypes.Contains(type))
                {
                    result.AddChild(new ObjectTreeModel
                        {
                            Name = property.Name,
                            Value = property.GetValue(context, null).With(x => x.ToString()).Return(x => x, "NULL")
                        });
                }
                else if (type == typeof(byte[]))
                {
                    result.AddChild(new ObjectTreeModel
                        {
                            Name = property.Name,
                            Value = "byte[]"
                        });                    
                }
                else if (type.IsArray)
                {
                    result.AddChild(new ObjectTreeModel
                        {
                            Name = property.Name,
                            Value = type.GetElementType().Name + "[]"
                        });                    
                }
                else if(type.IsClass)
                {
                    var subChild = new ObjectTreeModel
                        {
                            Name = property.Name
                        };
                    var value = property.GetValue(context, null);
                    if(value == null)
                        subChild.Value = "NULL";
                    else
                        AddPropertyRecursively(subChild, value);
                }
                else
                {
                    result.AddChild(new ObjectTreeModel
                        {
                            Name = property.Name,
                            Value = property.GetValue(context, null).With(x => x.ToString()).Return(x => x, "NULL")
                        });
                }
            }
        }

        private TaskSearchResultsModel BuildResultsByIteratorContext(string iteratorContext)
        {
            var taskSearchResponse = taskSearchClient.SearchNext(iteratorContext);

            var tasksIds = taskSearchResponse.Ids;
            var nextScrollId = taskSearchResponse.NextScrollId;
            return new TaskSearchResultsModel
                {
                    Tasks = remoteTaskQueue.GetTaskInfos(tasksIds).Select(x => new TaskModel
                        {
                            Id = x.Context.Id,
                            Name = x.Context.Name,
                            State = x.Context.State,
                            EnqueueTime = FromTicks(x.Context.Ticks),
                            StartExecutionTime = FromTicks(x.Context.StartExecutingTicks),
                            FinishExecutionTime = FromTicks(x.Context.FinishExecutingTicks),
                            MinimalStartTime = FromTicks(x.Context.FinishExecutingTicks),
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
                            MinimalStartTime = FromTicks(x.Context.FinishExecutingTicks),
                            AttemptCount = x.Context.Attempts,
                            ParentTaskId = x.Context.ParentTaskId,
                        }).ToArray(),
                    IteratorContext = nextScrollId,
                    TotalResultCount = total
                };
            return taskSearchResultsModel;
        }

        private static bool SearchConditionSuitableForSearch(TaskSearchConditionsModel taskSearchConditions)
        {
            return !(string.IsNullOrEmpty(taskSearchConditions.SearchString) && taskSearchConditions.TaskNames.ReturnArray().Length == 0 && taskSearchConditions.TaskStates.ReturnArray().Length == 0);
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