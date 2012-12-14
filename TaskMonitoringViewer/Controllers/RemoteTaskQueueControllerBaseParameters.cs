using RemoteQueue.Handling;

using SKBKontur.Catalogue.CassandraStorageCore.BusinessObjectStorageImpl;
using SKBKontur.Catalogue.Core.CommonBusinessObjects;
using SKBKontur.Catalogue.Core.Web.Controllers;
using SKBKontur.Catalogue.ObjectManipulation.Extender;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Controllers
{
    public class RemoteTaskQueueControllerBaseParameters
    {
        public RemoteTaskQueueControllerBaseParameters(
            LoggedInControllerBaseParameters loggedInControllerBaseParameters,
            ITaskMetadataModelBuilder taskMetadataModelBuilder,
            ITaskViewModelBuilder taskViewModelBuilder,
            IObjectValueExtracter objectValueExtracter,
            IMonitoringServiceStorage monitoringServiceStorage,
            IBusinessObjectsStorage businessObjectsStorage,
            ICatalogueExtender catalogueExtender,
            IMonitoringSearchRequestCriterionBuilder monitoringSearchRequestCriterionBuilder,
            IRemoteTaskQueue remoteTaskQueue)
        {
            RemoteTaskQueue = remoteTaskQueue;
            BusinessObjectsStorage = businessObjectsStorage;
            CatalogueExtender = catalogueExtender;
            MonitoringSearchRequestCriterionBuilder = monitoringSearchRequestCriterionBuilder;
            TaskMetadataModelBuilder = taskMetadataModelBuilder;
            LoggedInControllerBaseParameters = loggedInControllerBaseParameters;
            TaskViewModelBuilder = taskViewModelBuilder;
            ObjectValueExtracter = objectValueExtracter;
            MonitoringServiceStorage = monitoringServiceStorage;
        }

        public IRemoteTaskQueue RemoteTaskQueue { get; private set; }
        public IBusinessObjectsStorage BusinessObjectsStorage { get; private set; }
        public ICatalogueExtender CatalogueExtender { get; set; }
        public IMonitoringSearchRequestCriterionBuilder MonitoringSearchRequestCriterionBuilder { get; private set; }
        public LoggedInControllerBaseParameters LoggedInControllerBaseParameters { get; private set; }
        public ITaskMetadataModelBuilder TaskMetadataModelBuilder { get; private set; }
        public ITaskViewModelBuilder TaskViewModelBuilder { get; private set; }
        public IObjectValueExtracter ObjectValueExtracter { get; private set; }
        public IMonitoringServiceStorage MonitoringServiceStorage { get; private set; }
    }
}