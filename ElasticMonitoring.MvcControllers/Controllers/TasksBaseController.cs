using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

using Elasticsearch.Net;

using Humanizer;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions.Responses.Search;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Models;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage;

using ControllerBase = SKBKontur.Catalogue.Core.Web.Controllers.ControllerBase;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers
{
    public class TasksBaseController : ControllerBase
    {
        public TasksBaseController(
            TasksBaseControllerParameters parameters)
            : base(parameters.BaseParameters)
        {
            elasticsearchClient = parameters.ElasticsearchClientFactory.GetClient();
            taskDataNames = parameters.TaskDataRegistryBase.GetAllTaskDataInfos().Select(x => x.Value).Distinct().ToArray();
            taskStates = Enum.GetValues(typeof(TaskState)).Cast<TaskState>().Select(x => new KeyValuePair<string, string>(x.ToString(), x.Humanize())).ToArray();
            remoteTaskQueue = parameters.RemoteTaskQueue;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ViewResult Run(string q, string[] name, string[] state, string start, string end)
        {
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
            var taskSearchResultsModel = BuildResultsByIteratorContext(iteratorContext);
            return View("Scroll", taskSearchResultsModel);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ViewResult Details(string id)
        {
            var taskData = remoteTaskQueue.GetTaskInfo(id);
            return View("TaskDetails2", new TaskDetailsModel
                {
                    TaskName = taskData.Context.Name,
                    TaskId = taskData.Context.Id,
                    State = taskData.Context.State,
                    ExceptionInfo = taskData.With(x => x.ExceptionInfo).Return(x => x.ExceptionMessageInfo, ""),
                    DetailsTree = BuildDetailsTree(taskData)
                });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Cancel(string id)
        {
            remoteTaskQueue.CancelTask(id);
            return Json(new {Success = true});
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Rerun(string id)
        {
            remoteTaskQueue.RerunTask(id, TimeSpan.FromTicks(0));
            return Json(new {Success = true});
        }

        private ObjectTreeModel BuildDetailsTree(RemoteTaskInfo taskData)
        {
            var result = new ObjectTreeModel();
            AddPropertyValues(result, taskData.Context);
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
                Type type = property.PropertyType;
                if(type.IsEnum || plainTypes.Contains(type))
                {
                    result.AddChild(new ObjectTreeModel
                        {
                            Name = property.Name,
                            Value = property.GetValue(context, null).With(x => x.ToString()).Return(x => x, "NULL")
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
                    {
                        subChild.Value = "NULL";
                    }
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

        private void AddPropertyValues(ObjectTreeModel result, object context)
        {
            foreach(var property in context.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                result.AddChild(new ObjectTreeModel
                    {
                        Name = property.Name,
                        Value = property.GetValue(context, null).With(x => x.ToString()).Return(x => x, "NULL")
                    });
            }
        }

        private TaskSearchResultsModel BuildResultsByIteratorContext(string iteratorContext)
        {
            var scrollId = iteratorContext;
            var result = elasticsearchClient.Scroll<SearchResponseNoData>(scrollId, null, x => x.AddQueryString("scroll", "10m")).ProcessResponse();
            var tasksIds = result.Response.Hits.Hits.Select(x => x.Id).ToArray();
            var nextScrollId = result.Response.ScrollId;
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
            var searchString = taskSearchConditions.SearchString;
            var filters = new List<object>
                {
                    new
                        {
                            query_string = new
                                {
                                    query = searchString
                                },
                        }
                };
            if(taskSearchConditions.TaskNames.ReturnArray().Length > 0)
            {
                filters.Add(new
                    {
                        terms = new Dictionary<string, object>()
                            {
                                {"Meta.Name", taskSearchConditions.TaskNames},
                                {"minimum_should_match", 1}
                            }
                    });
            }
            if(taskSearchConditions.TaskStates.ReturnArray().Length > 0)
            {
                filters.Add(new
                    {
                        terms = new Dictionary<string, object>()
                            {
                                {"Meta.State", taskSearchConditions.TaskStates},
                                {"minimum_should_match", 1}
                            }
                    });
            }
            var metaResponse =
                elasticsearchClient
                    .Search<SearchResponseNoData>(IndexNameFactory.GetIndexForTimeRange(start, end), new
                        {
                            size = 100,
                            version = true,
                            _source = false,
                            query = new
                                {
                                    @bool = new
                                        {
                                            must = filters
                                        }
                                },
                            sort = new[]
                                {
                                    new Dictionary<string, object>
                                        {
                                            {"Meta.MinimalStartTime", new {order = "desc"}}
                                        }
                                }
                        }, x => x.IgnoreUnavailable(true).Scroll("10m").SearchType(SearchType.QueryThenFetch))
                    .ProcessResponse();
            var tasksIds = metaResponse.Response.Hits.Hits.Select(x => x.Id).ToArray();
            var total = metaResponse.Response.Hits.Total;
            var nextScrollId = metaResponse.Response.ScrollId;
            var taskSearchResultsModel = new TaskSearchResultsModel
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

        private readonly IElasticsearchClient elasticsearchClient;
        private readonly string[] taskDataNames;
        private readonly IRemoteTaskQueue remoteTaskQueue;
        private readonly KeyValuePair<string, string>[] taskStates;
    }
}