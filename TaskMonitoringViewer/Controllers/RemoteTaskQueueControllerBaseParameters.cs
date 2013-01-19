using RemoteQueue.Handling;

using SKBKontur.Catalogue.CassandraStorageCore.BusinessObjectStorageImpl;
using SKBKontur.Catalogue.Core.CommonBusinessObjects;
using SKBKontur.Catalogue.Core.Web.Controllers;
using SKBKontur.Catalogue.ObjectManipulation.Extender;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.TaskDetails;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.TaskList;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Controllers
{
    public class RemoteTaskQueueControllerBaseParameters
    {

        public RemoteTaskQueueControllerBaseParameters(
            LoggedInControllerBaseParameters loggedInControllerBaseParameters,
            ITaskMetadataModelBuilder taskMetadataModelBuilder,
            ITaskDetailsModelBuilder taskDetailsModelBuilder,
            ITaskDetailsHtmlModelBuilder taskDetailsHtmlModelBuilder,
            IObjectValueExtracter objectValueExtracter,
            IRemoteTaskQueueMonitoringServiceStorage remoteTaskQueueMonitoringServiceStorage,
            IBusinessObjectsStorage businessObjectsStorage,
            ICatalogueExtender catalogueExtender,
            IMonitoringSearchRequestCriterionBuilder monitoringSearchRequestCriterionBuilder,
            IRemoteTaskQueue remoteTaskQueue,
            ITaskListModelBuilder taskListModelBuilder,
            ITaskListHtmlModelBuilder taskListHtmlModelBuilder)
        {
            TaskListHtmlModelBuilder = taskListHtmlModelBuilder;
            TaskListModelBuilder = taskListModelBuilder;
            RemoteTaskQueue = remoteTaskQueue;
            BusinessObjectsStorage = businessObjectsStorage;
            CatalogueExtender = catalogueExtender;
            MonitoringSearchRequestCriterionBuilder = monitoringSearchRequestCriterionBuilder;
            TaskMetadataModelBuilder = taskMetadataModelBuilder;
            LoggedInControllerBaseParameters = loggedInControllerBaseParameters;
            TaskDetailsModelBuilder = taskDetailsModelBuilder;
            TaskDetailsHtmlModelBuilder = taskDetailsHtmlModelBuilder;
            ObjectValueExtracter = objectValueExtracter;
            RemoteTaskQueueMonitoringServiceStorage = remoteTaskQueueMonitoringServiceStorage;
        }

        public ITaskListModelBuilder TaskListModelBuilder { get; private set; }

        public IRemoteTaskQueue RemoteTaskQueue { get; private set; }
        public IBusinessObjectsStorage BusinessObjectsStorage { get; private set; }
        public ICatalogueExtender CatalogueExtender { get; set; }
        public IMonitoringSearchRequestCriterionBuilder MonitoringSearchRequestCriterionBuilder { get; private set; }
        public LoggedInControllerBaseParameters LoggedInControllerBaseParameters { get; private set; }
        public ITaskMetadataModelBuilder TaskMetadataModelBuilder { get; private set; }
        public ITaskDetailsModelBuilder TaskDetailsModelBuilder { get; private set; }
        public ITaskDetailsHtmlModelBuilder TaskDetailsHtmlModelBuilder { get; private set; }
        public IObjectValueExtracter ObjectValueExtracter { get; private set; }
        public IRemoteTaskQueueMonitoringServiceStorage RemoteTaskQueueMonitoringServiceStorage { get; private set; }
        public ITaskListHtmlModelBuilder TaskListHtmlModelBuilder { get; private set; }
    }
}