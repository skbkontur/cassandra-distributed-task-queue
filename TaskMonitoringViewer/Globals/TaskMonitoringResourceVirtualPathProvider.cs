using SKBKontur.Catalogue.Core.Web.Globals;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Globals
{
    public class TaskMonitoringResourceVirtualPathProvider : ResourceVirtualPathProviderBase
    {
        public TaskMonitoringResourceVirtualPathProvider()
            : base(typeof(TaskMonitoringResourceVirtualPathProvider).Assembly, "SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Views.", "~/TaskMonitoringViewer/")
        {
        }
    }
}