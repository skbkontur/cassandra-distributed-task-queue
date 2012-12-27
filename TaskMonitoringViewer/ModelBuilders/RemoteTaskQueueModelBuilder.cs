using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using SKBKontur.Catalogue.Core.CommonBusinessObjects;
using SKBKontur.Catalogue.Core.Web.Models.DateAndTimeModels;
using SKBKontur.Catalogue.Core.Web.PageModels;
using SKBKontur.Catalogue.Expressions;
using SKBKontur.Catalogue.ObjectManipulation.Extender;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities.Primitives;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Constants;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Controllers;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.Html;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders
{
    public class RemoteTaskQueueModelBuilder : IRemoteTaskQueueModelBuilder
    {
        public RemoteTaskQueueModelBuilder(IMonitoringServiceStorage monitoringServiceStorage,
                                           IBusinessObjectsStorage businessObjectsStorage,
                                           ICatalogueExtender extender,
                                           IMonitoringSearchRequestCriterionBuilder monitoringSearchRequestCriterionBuilder,
                                           IRemoteTaskQueueHtmlModelBuilder remoteTaskQueueHtmlModelBuilder)
        {
            this.monitoringServiceStorage = monitoringServiceStorage;
            this.businessObjectsStorage = businessObjectsStorage;
            this.extender = extender;
            this.monitoringSearchRequestCriterionBuilder = monitoringSearchRequestCriterionBuilder;
            this.remoteTaskQueueHtmlModelBuilder = remoteTaskQueueHtmlModelBuilder;
        }

        public RemoteTaskQueueModel Build(PageModelBaseParameters pageModelBaseParameters, int? pageNumber, string searchRequestId)
        {
            Expression<Func<MonitoringTaskMetadata, bool>> criterion = x => true;
            var names = monitoringServiceStorage.GetDistinctValues(criterion, x => x.Name).Cast<string>().ToArray();
            var states = monitoringServiceStorage.GetDistinctValues(criterion, x => x.State).Select(x => TryPrase<TaskState>((string)x)).ToArray();
            var allowedSearchValues = new AllowedSearchValues
                {
                    Names = names,
                    States = states,
                };

            var searchRequest = new MonitoringSearchRequest();
            if(!string.IsNullOrEmpty(searchRequestId))
            {
                if(!businessObjectsStorage.TryRead(searchRequestId, searchRequestId, out searchRequest))
                    searchRequest = new MonitoringSearchRequest();
            }
            extender.Extend(searchRequest);

            criterion = monitoringSearchRequestCriterionBuilder.And(criterion, monitoringSearchRequestCriterionBuilder.BuildCriterion(searchRequest));

            int page = (pageNumber ?? 0);
            var countPerPage = ControllerConstants.DefaultRecordsNumberPerPage;
            var rangeFrom = page * ControllerConstants.DefaultRecordsNumberPerPage;
            var cnt = monitoringServiceStorage.GetCount(criterion);
            var totalPagesCount = (cnt + countPerPage - 1) / countPerPage;
            var fullTaskMetaInfos = monitoringServiceStorage.RangeSearch(criterion, rangeFrom, countPerPage, x => x.MinimalStartTicks.Descending());
            var remoteTaskQueueModelData = new RemoteTaskQueuePageModel
                {
                    SearchPanel = new SearchPanelModelData
                        {
                            States = BuildArray(allowedSearchValues.States, searchRequest.States),
                            TaskName = searchRequest.Name,
                            TaskId = searchRequest.TaskId,
                            ParentTaskId = searchRequest.ParentTaskId,
                            AllowedTaskNames = allowedSearchValues.Names,
                            Ticks = new DateTimeRangeModel
                                {
                                    From = DateAndTime.Create(searchRequest.Ticks.From),
                                    To = DateAndTime.Create(searchRequest.Ticks.To)
                                },
                            StartExecutedTicks = new DateTimeRangeModel
                                {
                                    From = DateAndTime.Create(searchRequest.StartExecutingTicks.From),
                                    To = DateAndTime.Create(searchRequest.StartExecutingTicks.To)
                                },
                            MinimalStartTicks = new DateTimeRangeModel
                                {
                                    From = DateAndTime.Create(searchRequest.MinimalStartTicks.From),
                                    To = DateAndTime.Create(searchRequest.MinimalStartTicks.To)
                                }
                        },
                    TaskModels = fullTaskMetaInfos.Select(x => new TaskMetaInfoModel
                        {
                            Attempts = x.Attempts,
                            TaskId = x.TaskId,
                            Name = x.Name,
                            State = x.State,
                            EnqueueTicks = x.Ticks.Ticks.ToString(),
                            ParentTaskId = x.ParentTaskId,
                            StartExecutedTicks = x.StartExecutingTicks.ToString(),
                            MinimalStartTicks = x.MinimalStartTicks.Ticks.ToString()
                        }).ToArray(),
                };
            var model = new RemoteTaskQueueModel(pageModelBaseParameters, remoteTaskQueueModelData)
                {
                    PageNumber = page,
                    TotalPagesCount = totalPagesCount,
                    PagesWindowSize = 3,
                    SearchRequestId = searchRequestId ?? ""
                };
            model.HtmlModel = remoteTaskQueueHtmlModelBuilder.Build(model);
            return model;
        }


        private Pair<T, bool?> [] BuildArray<T>(T[] allowedValues, T[] requestValues, HashSet<T> needValues = null)
            where T : IComparable
        {
            var dictionary = allowedValues.ToDictionary(x => x, x => false);
            foreach (var requestValue in requestValues)
            {
                if (dictionary.ContainsKey(requestValue))
                    dictionary[requestValue] = true;
                else
                    dictionary.Add(requestValue, true);
            }
            var array = dictionary.Select(x => new Pair<T, bool?> { Key = x.Key, Value = x.Value }).ToArray();
            Array.Sort(array, (x, y) => x.Key.CompareTo(y.Key));
            return array;
        }

        private T TryPrase<T>(string s) where T : struct
        {
            T res;
            return !Enum.TryParse(s, true, out res) ? default(T) : res;
        }

        private readonly IMonitoringServiceStorage monitoringServiceStorage;
        private readonly IBusinessObjectsStorage businessObjectsStorage;
        private readonly ICatalogueExtender extender;
        private readonly IMonitoringSearchRequestCriterionBuilder monitoringSearchRequestCriterionBuilder;
        private readonly IRemoteTaskQueueHtmlModelBuilder remoteTaskQueueHtmlModelBuilder;
    }
}