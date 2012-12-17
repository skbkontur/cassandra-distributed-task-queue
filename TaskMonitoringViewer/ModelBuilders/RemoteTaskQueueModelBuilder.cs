using System;
using System.Linq;
using System.Linq.Expressions;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.Core.CommonBusinessObjects;
using SKBKontur.Catalogue.Core.Web.PageModels;
using SKBKontur.Catalogue.Expressions;
using SKBKontur.Catalogue.ObjectManipulation.Extender;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;
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
            var remoteTaskQueueModelData = new RemoteTaskQueueModelData
                {
                    SearchPanel = new SearchPanelModelData
                        {
                            TaskName = searchRequest.Name
                        },
                    TaskModels = fullTaskMetaInfos.Select(x => new TaskMetaInfoModel
                        {
                            Attempts = x.Attempts,
                            TaskId = x.TaskId,
                            Name = x.Name,
                            State = x.State,
                            EnqueueTicks = TicksToDateString(x.Ticks),
                            ParentTaskId = x.ParentTaskId,
                            StartExecutedTicks = TicksToDateString(x.StartExecutingTicks),
                            MinimalStartTicks = TicksToDateString(x.MinimalStartTicks)
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

        private string TicksToDateString(long? ticks)
        {
            return ticks == null ? null : new DateTime(ticks.Value).ToString();
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