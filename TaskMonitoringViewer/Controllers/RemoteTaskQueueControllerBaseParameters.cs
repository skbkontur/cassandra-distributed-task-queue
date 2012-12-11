using SKBKontur.Catalogue.CassandraStorageCore.BusinessObjectStorageImpl;
using SKBKontur.Catalogue.Core.Web.Controllers;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Controllers
{
    public class RemoteTaskQueueControllerBaseParameters
    {
        public RemoteTaskQueueControllerBaseParameters(
            LoggedInControllerBaseParameters loggedInControllerBaseParameters,
            ITaskMetaInfoModelBuilder taskMetaInfoModelBuilder,
            ITaskViewModelBuilder taskViewModelBuilder,
            IObjectValueExtracter objectValueExtracter,
            IMonitoringServiceClient monitoringServiceClient)
        {
            TaskMetaInfoModelBuilder = taskMetaInfoModelBuilder;
            LoggedInControllerBaseParameters = loggedInControllerBaseParameters;
            TaskViewModelBuilder = taskViewModelBuilder;
            ObjectValueExtracter = objectValueExtracter;
            MonitoringServiceClient = monitoringServiceClient;
        }

        public LoggedInControllerBaseParameters LoggedInControllerBaseParameters { get; private set; }
        public ITaskMetaInfoModelBuilder TaskMetaInfoModelBuilder { get; private set; }
        public ITaskViewModelBuilder TaskViewModelBuilder { get; private set; }
        public IObjectValueExtracter ObjectValueExtracter { get; private set; }
        public IMonitoringServiceClient MonitoringServiceClient { get; private set; }
    }
}