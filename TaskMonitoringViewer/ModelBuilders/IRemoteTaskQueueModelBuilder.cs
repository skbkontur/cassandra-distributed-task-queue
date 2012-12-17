using SKBKontur.Catalogue.Core.Web.PageModels;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders
{
    public interface IRemoteTaskQueueModelBuilder
    {
        RemoteTaskQueueModel Build(PageModelBaseParameters pageModelBaseParameters, int? pageNumber, string searchRequestId);
    }
}