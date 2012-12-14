using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.AccessControl.AccessRules;
using SKBKontur.Catalogue.CassandraStorageCore.BusinessObjectStorageImpl;
using SKBKontur.Catalogue.Core.CommonBusinessObjects;
using SKBKontur.Catalogue.Core.Web.Controllers;
using SKBKontur.Catalogue.Expressions;
using SKBKontur.Catalogue.ObjectManipulation.Extender;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Constants;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Controllers
{
    [ResourseGroup(ResourseGroups.AdminResourse)]
    public abstract class RemoteTaskQueueControllerBase : AuthanticatedControllerBase
    {
        protected RemoteTaskQueueControllerBase(RemoteTaskQueueControllerBaseParameters remoteTaskQueueControllerBaseParameters)
            : base(remoteTaskQueueControllerBaseParameters.LoggedInControllerBaseParameters)
        {
            taskMetadataModelBuilder = remoteTaskQueueControllerBaseParameters.TaskMetadataModelBuilder;
            taskViewModelBuilder = remoteTaskQueueControllerBaseParameters.TaskViewModelBuilder;
            objectValueExtracter = remoteTaskQueueControllerBaseParameters.ObjectValueExtracter;
            monitoringServiceStorage = remoteTaskQueueControllerBaseParameters.MonitoringServiceStorage;
            businessObjectsStorage = remoteTaskQueueControllerBaseParameters.BusinessObjectsStorage;
            extender = remoteTaskQueueControllerBaseParameters.CatalogueExtender;
            monitoringSearchRequestCriterionBuilder = remoteTaskQueueControllerBaseParameters.MonitoringSearchRequestCriterionBuilder;
            remoteTaskQueue = remoteTaskQueueControllerBaseParameters.RemoteTaskQueue;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Run(int? pageNumber, string searchRequestId)
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
                businessObjectsStorage.TryRead(searchRequestId, searchRequestId, out searchRequest);
            extender.Extend(searchRequest);

            criterion = monitoringSearchRequestCriterionBuilder.And(criterion, monitoringSearchRequestCriterionBuilder.BuildCriterion(searchRequest));

            int page = (pageNumber ?? 0);
            var countPerPage = ControllerConstants.DefaultRecordsNumberPerPage;
            var rangeFrom = page * ControllerConstants.DefaultRecordsNumberPerPage;
            var totalPagesCount = (monitoringServiceStorage.GetCount(criterion) + countPerPage - 1) / countPerPage;
            var fullTaskMetaInfos = monitoringServiceStorage.RangeSearch(criterion, rangeFrom, countPerPage, x => x.MinimalStartTicks.Descending());

            var model = new RemoteTaskQueueModel
                {
                    PageNumber = page,
                    TotalPagesCount = totalPagesCount,
                    PagesWindowSize = 3,
                    TaskModels = fullTaskMetaInfos.Select(x => new TaskMetaInfoModel
                        {
                            Attempts = x.Attempts,
                            TaskId = x.TaskId,
                            Name = x.Name,
                            State = x.State,
                        }).ToArray()
                }; //PageModelBaseParameters, RemoteTaskQueueModelBuilder.BuildModel(LanguageProvider, fullTaskMetaInfos, searchRequest, allowedSearchValues));
            return View("RemoteTaskQueueListView", model);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Show(string id)
        {
            var remoteTaskInfo = remoteTaskQueue.GetTaskInfo(id);
            return View("RemoteTaskQueueDetailsView", taskViewModelBuilder.Build(remoteTaskInfo));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Cancel(string id)
        {
            if(!remoteTaskQueue.CancelTask(id))
                return ReturnOperationResult(new UnsuccessOperationResult {ErrorMessage = "Задача не может быть отменена"});
            return ReturnOperationResult(new SuccessOperationResult
                {
                    ActionDescription = "Задача успешно отменена",
                    NeedRedirect = true,
                    RedirectTo = Request.Headers["Referer"]
                });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Rerun(string id)
        {
            if(!remoteTaskQueue.RerunTask(id, TimeSpan.FromTicks(0)))
                return ReturnOperationResult(new UnsuccessOperationResult {ErrorMessage = "Задача не может перезапущена"});
            return ReturnOperationResult(new SuccessOperationResult
                {
                    ActionDescription = "Задача успешно перезапущена",
                    NeedRedirect = true,
                    RedirectTo = Request.Headers["Referer"]
                });
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetBytes(string id, string path)
        {
            var taskData = remoteTaskQueue.GetTaskInfo(id).TaskData;
            var value = objectValueExtracter.Extract(taskData.GetType(), taskData, path);
            if(value.GetType() != typeof(byte[]))
                throw new Exception(string.Format("Type of property by path '{0}' has type '{1}' instead of '{2}'", path, value.GetType(), typeof(byte[])));
            var fileDownloadName = string.Format("{0}_{1}.xml", DateTime.UtcNow.ToString("yyyy.MM.dd hh:mm:ss"), Guid.NewGuid());
            return File((byte[])value, "application/xml", fileDownloadName);
        }

        private T TryPrase<T>(string s) where T : struct
        {
            T res;
            return !Enum.TryParse(s, true, out res) ? default(T) : res;
        }

        private readonly ITaskMetadataModelBuilder taskMetadataModelBuilder;
        private readonly ITaskViewModelBuilder taskViewModelBuilder;
        private readonly IObjectValueExtracter objectValueExtracter;
        private readonly IMonitoringServiceStorage monitoringServiceStorage;
        private readonly IBusinessObjectsStorage businessObjectsStorage;
        private readonly ICatalogueExtender extender;
        private readonly IMonitoringSearchRequestCriterionBuilder monitoringSearchRequestCriterionBuilder;
        private readonly IRemoteTaskQueue remoteTaskQueue;
    }
}