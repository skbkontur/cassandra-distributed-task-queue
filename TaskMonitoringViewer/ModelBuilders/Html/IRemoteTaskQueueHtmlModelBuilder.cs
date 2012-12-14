using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Html;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.Html
{
    public interface IRemoteTaskQueueHtmlModelBuilder
    {
        SearchPanelHtmlModel Build(RemoteTaskQueueModel pageModel);
    }
}