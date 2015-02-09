using RemoteQueue.Handling;

using SKBKontur.Catalogue.Core.CommonBusinessObjects;
using SKBKontur.Catalogue.Core.Web.Controllers;
using SKBKontur.Catalogue.Core.Web.Models.ModelConfigurations;
using SKBKontur.Catalogue.ObjectManipulation.Extender;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.TaskDetails;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.TaskList;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskList;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Controllers
{
    public class RemoteTaskQueueControllerBaseParameters
    {
        public RemoteTaskQueueControllerBaseParameters(
            ControllerBaseParameters controllerBaseParameters,
            ITaskDetailsModelBuilder taskDetailsModelBuilder,
            ITaskDetailsHtmlModelBuilder taskDetailsHtmlModelBuilder,
            IRemoteTaskQueueMonitoringServiceStorage remoteTaskQueueMonitoringServiceStorage,
            IBusinessObjectStorage businessObjectsStorage,
            ICatalogueExtender catalogueExtender,
            IMonitoringSearchRequestCriterionBuilder monitoringSearchRequestCriterionBuilder,
            IRemoteTaskQueue remoteTaskQueue,
            ITaskListModelBuilder taskListModelBuilder,
            ITaskListHtmlModelBuilder taskListHtmlModelBuilder,
            IWebMutatorsTreeCollection<TaskListModelData> webMutatorsTreeCollection)
        {
            ControllerBaseParameters = controllerBaseParameters;
            TaskListHtmlModelBuilder = taskListHtmlModelBuilder;
            TaskListModelBuilder = taskListModelBuilder;
            RemoteTaskQueue = remoteTaskQueue;
            BusinessObjectsStorage = businessObjectsStorage;
            CatalogueExtender = catalogueExtender;
            MonitoringSearchRequestCriterionBuilder = monitoringSearchRequestCriterionBuilder;
            TaskDetailsModelBuilder = taskDetailsModelBuilder;
            TaskDetailsHtmlModelBuilder = taskDetailsHtmlModelBuilder;
            RemoteTaskQueueMonitoringServiceStorage = remoteTaskQueueMonitoringServiceStorage;
            WebMutatorsTreeCollection = webMutatorsTreeCollection;
        }

        public ControllerBaseParameters ControllerBaseParameters { get; private set; }
        public ITaskListModelBuilder TaskListModelBuilder { get; private set; }
        public IRemoteTaskQueue RemoteTaskQueue { get; private set; }
        public IBusinessObjectStorage BusinessObjectsStorage { get; private set; }
        public ICatalogueExtender CatalogueExtender { get; private set; }
        public IMonitoringSearchRequestCriterionBuilder MonitoringSearchRequestCriterionBuilder { get; private set; }
        public ITaskDetailsModelBuilder TaskDetailsModelBuilder { get; private set; }
        public ITaskDetailsHtmlModelBuilder TaskDetailsHtmlModelBuilder { get; private set; }
        public IRemoteTaskQueueMonitoringServiceStorage RemoteTaskQueueMonitoringServiceStorage { get; private set; }
        public ITaskListHtmlModelBuilder TaskListHtmlModelBuilder { get; private set; }
        public IWebMutatorsTreeCollection<TaskListModelData> WebMutatorsTreeCollection { get; private set; }
    }
}